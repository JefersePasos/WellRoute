using Microsoft.EntityFrameworkCore;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Dtos;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Favoritos;

public interface IFavoritoService
{
    // Destinos activos que el usuario guardó, del más reciente al más antiguo.
    Task<List<DestinoListaDto>> ListarAsync(int usuarioId, CancellationToken ct);

    // Solo los IDs: sirve para marcar el corazón en la portada.
    Task<List<int>> ListarIdsAsync(int usuarioId, CancellationToken ct);

    Task<ResultadoOperacion<bool>> AgregarAsync(int usuarioId, int destinoId, CancellationToken ct);

    Task<ResultadoOperacion<bool>> QuitarAsync(int usuarioId, int destinoId, CancellationToken ct);
}

public class FavoritoService(WellRouteDbContext db, IAuditoriaService auditoria) : IFavoritoService
{
    public async Task<List<DestinoListaDto>> ListarAsync(int usuarioId, CancellationToken ct)
    {
        return await db.Favoritos
            .AsNoTracking()
            .Where(f => f.UsuarioID == usuarioId && f.Destino.Activo)
            .OrderByDescending(f => f.FechaCreacion)
            .Select(f => new DestinoListaDto
            {
                DestinoID = f.DestinoID,
                Nombre = f.Destino.Nombre,
                Pais = f.Destino.Pais,
                Categoria = f.Destino.Categoria,
                ImagenUrl = f.Destino.ImagenUrl,
                Activo = f.Destino.Activo
            })
            .ToListAsync(ct);
    }

    public async Task<List<int>> ListarIdsAsync(int usuarioId, CancellationToken ct)
    {
        return await db.Favoritos
            .AsNoTracking()
            .Where(f => f.UsuarioID == usuarioId && f.Destino.Activo)
            .Select(f => f.DestinoID)
            .ToListAsync(ct);
    }

    public async Task<ResultadoOperacion<bool>> AgregarAsync(int usuarioId, int destinoId, CancellationToken ct)
    {
        var destino = await db.Destinos
            .AsNoTracking()
            .Where(d => d.DestinoID == destinoId && d.Activo)
            .Select(d => new { d.Nombre })
            .FirstOrDefaultAsync(ct);

        if (destino is null)
            return ResultadoOperacion<bool>.Fallo(EstadoOperacion.NoEncontrado, "El destino no existe.");

        // Si ya era favorito no hay nada que hacer.
        if (await db.Favoritos.AnyAsync(f => f.UsuarioID == usuarioId && f.DestinoID == destinoId, ct))
            return ResultadoOperacion<bool>.Ok(true);

        db.Favoritos.Add(new Favorito
        {
            UsuarioID = usuarioId,
            DestinoID = destinoId,
            FechaCreacion = DateTime.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Dos clics casi al mismo tiempo: el otro ya lo guardó.
            return ResultadoOperacion<bool>.Ok(true);
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Favoritos, AccionesBitacora.Crear,
            "Favorito", destinoId.ToString(), $"Destino guardado como favorito: {destino.Nombre}");

        return ResultadoOperacion<bool>.Ok(true);
    }

    public async Task<ResultadoOperacion<bool>> QuitarAsync(int usuarioId, int destinoId, CancellationToken ct)
    {
        var favorito = await db.Favoritos
            .Include(f => f.Destino)
            .FirstOrDefaultAsync(f => f.UsuarioID == usuarioId && f.DestinoID == destinoId, ct);

        // Si no estaba, el resultado es el mismo que se quería.
        if (favorito is null)
            return ResultadoOperacion<bool>.Ok(true);

        var nombre = favorito.Destino.Nombre;

        db.Favoritos.Remove(favorito);
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(
            ModulosBitacora.Favoritos, AccionesBitacora.Eliminar,
            "Favorito", destinoId.ToString(), $"Destino quitado de favoritos: {nombre}");

        return ResultadoOperacion<bool>.Ok(true);
    }
}
