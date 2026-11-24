using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Dtos;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Destinos;

public interface IDestinoService
{
    // Portada y ficha pública: solo destinos activos. Se puede buscar por texto y filtrar por categoría.
    Task<List<DestinoListaDto>> ListarPublicosAsync(string? buscar, string? categoria, CancellationToken ct);

    Task<ResultadoOperacion<DestinoDetalleDto>> ObtenerPublicoAsync(int id, CancellationToken ct);

    // Gestión: incluye los inactivos.
    Task<List<DestinoListaDto>> ListarGestionAsync(CancellationToken ct);

    Task<ResultadoOperacion<DestinoDetalleDto>> ObtenerGestionAsync(int id, CancellationToken ct);

    Task<ResultadoOperacion<DestinoDetalleDto>> CrearAsync(GuardarDestinoRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<DestinoDetalleDto>> ActualizarAsync(int id, GuardarDestinoRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct);
}

public class DestinoService(WellRouteDbContext db, IAuditoriaService auditoria) : IDestinoService
{
    // Palabras que se ignoran al buscar, para que "Quiero ir a París" encuentre "París".
    private static readonly HashSet<string> PalabrasVacias = new(StringComparer.OrdinalIgnoreCase)
    {
        "quiero", "ir", "a", "al", "en", "de", "del", "la", "el", "los", "las", "un", "una",
        "viajar", "conocer", "visitar", "donde", "dónde", "para", "me", "gustaria", "gustaría", "voy", "por"
    };

    public async Task<List<DestinoListaDto>> ListarPublicosAsync(
        string? buscar, string? categoria, CancellationToken ct)
    {
        var consulta = db.Destinos.AsNoTracking().Where(d => d.Activo);

        var categoriaOficial = CategoriasDestino.Todas.FirstOrDefault(c =>
            string.Equals(c, categoria?.Trim(), StringComparison.OrdinalIgnoreCase));

        if (categoriaOficial is not null)
            consulta = consulta.Where(d => d.Categoria == categoriaOficial);

        var lista = await consulta
            .OrderBy(d => d.Nombre)
            .Select(d => new DestinoListaDto
            {
                DestinoID = d.DestinoID,
                Nombre = d.Nombre,
                Pais = d.Pais,
                Categoria = d.Categoria,
                ImagenUrl = d.ImagenUrl,
                Activo = d.Activo
            })
            .ToListAsync(ct);

        var terminos = ExtraerTerminos(buscar);
        if (terminos.Count == 0)
            return lista;

        // Se filtra en memoria para ignorar mayúsculas y tildes ("paris" encuentra "París").
        return lista
            .Where(d => terminos.All(t => Contiene(d.Nombre, t) || Contiene(d.Pais, t) || Contiene(d.Categoria, t)))
            .ToList();
    }

    public async Task<List<DestinoListaDto>> ListarGestionAsync(CancellationToken ct)
    {
        return await db.Destinos
            .AsNoTracking()
            .OrderBy(d => d.Nombre)
            .Select(d => new DestinoListaDto
            {
                DestinoID = d.DestinoID,
                Nombre = d.Nombre,
                Pais = d.Pais,
                Categoria = d.Categoria,
                ImagenUrl = d.ImagenUrl,
                Activo = d.Activo
            })
            .ToListAsync(ct);
    }

    public Task<ResultadoOperacion<DestinoDetalleDto>> ObtenerPublicoAsync(int id, CancellationToken ct) =>
        ObtenerAsync(id, soloActivos: true, ct);

    public Task<ResultadoOperacion<DestinoDetalleDto>> ObtenerGestionAsync(int id, CancellationToken ct) =>
        ObtenerAsync(id, soloActivos: false, ct);

    public async Task<ResultadoOperacion<DestinoDetalleDto>> CrearAsync(
        GuardarDestinoRequest solicitud, CancellationToken ct)
    {
        var (categoria, error) = Validar(solicitud);
        if (error is not null)
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(EstadoOperacion.Invalido, error);

        var nombre = solicitud.Nombre.Trim();
        var pais = solicitud.Pais.Trim();

        if (await db.Destinos.AnyAsync(d => d.Nombre == nombre && d.Pais == pais, ct))
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "Ya existe un destino con ese nombre en ese país.");

        var destino = new Destino
        {
            Nombre = nombre,
            Pais = pais,
            Categoria = categoria!,
            Descripcion = solicitud.Descripcion.Trim(),
            ImagenUrl = solicitud.ImagenUrl.Trim(),
            VideoUrl = LimpiarOpcional(solicitud.VideoUrl),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        db.Destinos.Add(destino);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el destino. Verifica que no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Destinos, AccionesBitacora.Crear,
            "Destino", destino.DestinoID.ToString(), $"Alta del destino {nombre} ({pais})");

        return await ObtenerGestionAsync(destino.DestinoID, ct);
    }

    public async Task<ResultadoOperacion<DestinoDetalleDto>> ActualizarAsync(
        int id, GuardarDestinoRequest solicitud, CancellationToken ct)
    {
        var destino = await db.Destinos.FirstOrDefaultAsync(d => d.DestinoID == id, ct);

        if (destino is null)
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El destino no existe.");

        var (categoria, error) = Validar(solicitud);
        if (error is not null)
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(EstadoOperacion.Invalido, error);

        var nombre = solicitud.Nombre.Trim();
        var pais = solicitud.Pais.Trim();

        if (await db.Destinos.AnyAsync(d => d.Nombre == nombre && d.Pais == pais && d.DestinoID != id, ct))
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "Ya existe otro destino con ese nombre en ese país.");

        destino.Nombre = nombre;
        destino.Pais = pais;
        destino.Categoria = categoria!;
        destino.Descripcion = solicitud.Descripcion.Trim();
        destino.ImagenUrl = solicitud.ImagenUrl.Trim();
        destino.VideoUrl = LimpiarOpcional(solicitud.VideoUrl);
        destino.FechaActualizacion = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<DestinoDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el destino. Verifica que no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Destinos, AccionesBitacora.Editar,
            "Destino", id.ToString(), $"Edición del destino {nombre} ({pais})");

        return await ObtenerGestionAsync(id, ct);
    }

    public async Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct)
    {
        var destino = await db.Destinos.FirstOrDefaultAsync(d => d.DestinoID == id, ct);

        if (destino is null)
            return ResultadoOperacion<bool>.Fallo(EstadoOperacion.NoEncontrado, "El destino no existe.");

        if (destino.Activo == activo)
            return ResultadoOperacion<bool>.Ok(true);

        destino.Activo = activo;
        destino.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(
            ModulosBitacora.Destinos,
            activo ? AccionesBitacora.Activar : AccionesBitacora.Desactivar,
            "Destino", id.ToString(),
            $"{(activo ? "Activación" : "Desactivación")} del destino {destino.Nombre}");

        return ResultadoOperacion<bool>.Ok(true);
    }

    private async Task<ResultadoOperacion<DestinoDetalleDto>> ObtenerAsync(
        int id, bool soloActivos, CancellationToken ct)
    {
        var destino = await db.Destinos
            .AsNoTracking()
            .Where(d => d.DestinoID == id && (!soloActivos || d.Activo))
            .Select(d => new DestinoDetalleDto
            {
                DestinoID = d.DestinoID,
                Nombre = d.Nombre,
                Pais = d.Pais,
                Categoria = d.Categoria,
                Descripcion = d.Descripcion,
                ImagenUrl = d.ImagenUrl,
                VideoUrl = d.VideoUrl,
                Activo = d.Activo,
                FechaCreacion = d.FechaCreacion
            })
            .FirstOrDefaultAsync(ct);

        return destino is null
            ? ResultadoOperacion<DestinoDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El destino no existe.")
            : ResultadoOperacion<DestinoDetalleDto>.Ok(destino);
    }

    // Devuelve la categoría con su escritura oficial, o un mensaje de error.
    private static (string? Categoria, string? Error) Validar(GuardarDestinoRequest solicitud)
    {
        var categoria = CategoriasDestino.Todas.FirstOrDefault(c =>
            string.Equals(c, solicitud.Categoria.Trim(), StringComparison.OrdinalIgnoreCase));

        if (categoria is null)
            return (null, "La categoría no es válida.");

        // Solo http o https: así nunca se guarda un enlace tipo javascript: que luego se pinte en la página.
        if (!EsUrlHttp(solicitud.ImagenUrl))
            return (null, "La URL de la imagen debe empezar con http:// o https://.");

        if (!string.IsNullOrWhiteSpace(solicitud.VideoUrl) && !EsUrlHttp(solicitud.VideoUrl))
            return (null, "La URL del video debe empezar con http:// o https://.");

        return (categoria, null);
    }

    private static bool EsUrlHttp(string? valor) =>
        Uri.TryCreate(valor?.Trim(), UriKind.Absolute, out var uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);

    private static string? LimpiarOpcional(string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

    // Separa lo que escribió el usuario en palabras útiles (máximo 5, de hasta 50 caracteres).
    private static List<string> ExtraerTerminos(string? buscar)
    {
        if (string.IsNullOrWhiteSpace(buscar))
            return new List<string>();

        return buscar
            .Split(new[] { ' ', ',', '.', ';', '¿', '?', '¡', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(t => !PalabrasVacias.Contains(t))
            .Select(t => t.Length > 50 ? t[..50] : t)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .ToList();
    }

    private static bool Contiene(string texto, string termino) =>
        CultureInfo.InvariantCulture.CompareInfo.IndexOf(
            texto, termino, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;
}
