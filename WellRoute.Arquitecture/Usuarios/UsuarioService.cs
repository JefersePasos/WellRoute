using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.Arquitecture.Roles;
using WellRoute.Arquitecture.Security;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Dtos;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Usuarios;

public interface IUsuarioService
{
    Task<List<UsuarioListaDto>> ListarAsync(CancellationToken ct);

    Task<ResultadoOperacion<UsuarioDetalleDto>> ObtenerAsync(int id, CancellationToken ct);

    Task<ResultadoOperacion<ContrasenaTemporalDto>> CrearAsync(GuardarUsuarioRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<UsuarioDetalleDto>> ActualizarAsync(int id, GuardarUsuarioRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int id, bool activo, int? actorId, CancellationToken ct);

    Task<ResultadoOperacion<ContrasenaTemporalDto>> RestablecerContrasenaAsync(int id, CancellationToken ct);
}

public class UsuarioService(
    WellRouteDbContext db,
    IPasswordHasher hasher,
    IAuditoriaService auditoria) : IUsuarioService
{
    public async Task<List<UsuarioListaDto>> ListarAsync(CancellationToken ct)
    {
        return await db.Usuarios
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .Select(u => new UsuarioListaDto
            {
                UsuarioID = u.UsuarioID,
                Nombre = u.Nombre,
                Email = u.Email,
                Activo = u.Activo,
                FechaUltimoLogin = u.FechaUltimoLogin,
                Roles = u.UsuarioRoles.Select(ur => ur.Rol.Nombre).ToList()
            })
            .ToListAsync(ct);
    }

    public async Task<ResultadoOperacion<UsuarioDetalleDto>> ObtenerAsync(int id, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .AsNoTracking()
            .Where(u => u.UsuarioID == id)
            .Select(u => new UsuarioDetalleDto
            {
                UsuarioID = u.UsuarioID,
                Nombre = u.Nombre,
                Email = u.Email,
                Telefono = u.Telefono,
                Activo = u.Activo,
                DebeCambiarContrasena = u.DebeCambiarContrasena,
                FechaCreacion = u.FechaCreacion,
                FechaUltimoLogin = u.FechaUltimoLogin,
                RolIds = u.UsuarioRoles.Select(ur => ur.RolID).ToList()
            })
            .FirstOrDefaultAsync(ct);

        return usuario is null
            ? ResultadoOperacion<UsuarioDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El usuario no existe.")
            : ResultadoOperacion<UsuarioDetalleDto>.Ok(usuario);
    }

    public async Task<ResultadoOperacion<ContrasenaTemporalDto>> CrearAsync(
        GuardarUsuarioRequest solicitud, CancellationToken ct)
    {
        var email = solicitud.Email.Trim();
        var rolIds = solicitud.RolIds.Distinct().ToList();

        var errorRoles = await ValidarRolesAsync(rolIds, ct);
        if (errorRoles is not null)
            return ResultadoOperacion<ContrasenaTemporalDto>.Fallo(EstadoOperacion.Invalido, errorRoles);

        if (await db.Usuarios.AnyAsync(u => u.Email == email, ct))
            return ResultadoOperacion<ContrasenaTemporalDto>.Fallo(
                EstadoOperacion.Conflicto, "Ya existe un usuario con ese correo.");

        var temporal = GenerarContrasenaTemporal();
        var (hash, salt) = hasher.Hashear(temporal);
        var ahora = DateTime.UtcNow;

        var usuario = new Usuario
        {
            Nombre = solicitud.Nombre.Trim(),
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            Telefono = solicitud.Telefono?.Trim() ?? string.Empty,
            FotoPerfil = string.Empty,
            Activo = true,
            FechaCreacion = ahora,
            DebeCambiarContrasena = true
        };

        foreach (var rolId in rolIds)
            usuario.UsuarioRoles.Add(new Usuario_Rol { RolID = rolId, FechaAsignacion = ahora });

        db.Usuarios.Add(usuario);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<ContrasenaTemporalDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el usuario. Verifica que el correo no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios, AccionesBitacora.Crear,
            "Usuario", usuario.UsuarioID.ToString(), $"Alta del usuario {email}");

        return ResultadoOperacion<ContrasenaTemporalDto>.Ok(new ContrasenaTemporalDto
        {
            UsuarioID = usuario.UsuarioID,
            Email = usuario.Email,
            ContrasenaTemporal = temporal
        });
    }

    public async Task<ResultadoOperacion<UsuarioDetalleDto>> ActualizarAsync(
        int id, GuardarUsuarioRequest solicitud, CancellationToken ct)
    {
        var usuario = await db.Usuarios
            .Include(u => u.UsuarioRoles)
            .FirstOrDefaultAsync(u => u.UsuarioID == id, ct);

        if (usuario is null)
            return ResultadoOperacion<UsuarioDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El usuario no existe.");

        var email = solicitud.Email.Trim();
        var rolIds = solicitud.RolIds.Distinct().ToList();

        var errorRoles = await ValidarRolesAsync(rolIds, ct);
        if (errorRoles is not null)
            return ResultadoOperacion<UsuarioDetalleDto>.Fallo(EstadoOperacion.Invalido, errorRoles);

        if (await db.Usuarios.AnyAsync(u => u.Email == email && u.UsuarioID != id, ct))
            return ResultadoOperacion<UsuarioDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "Ya existe otro usuario con ese correo.");

        // Siempre debe quedar al menos un administrador activo.
        var rolAdminId = await ObtenerRolAdminIdAsync(ct);
        var eraAdmin = rolAdminId.HasValue && usuario.UsuarioRoles.Any(ur => ur.RolID == rolAdminId.Value);
        var seguiraAdmin = rolAdminId.HasValue && rolIds.Contains(rolAdminId.Value);

        if (eraAdmin && !seguiraAdmin && !await HayOtroAdminActivoAsync(id, ct))
            return ResultadoOperacion<UsuarioDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "Debe quedar al menos un administrador activo.");

        var ahora = DateTime.UtcNow;

        usuario.Nombre = solicitud.Nombre.Trim();
        usuario.Email = email;
        usuario.Telefono = solicitud.Telefono?.Trim() ?? string.Empty;
        usuario.FechaActualizacion = ahora;

        foreach (var ur in usuario.UsuarioRoles.Where(x => !rolIds.Contains(x.RolID)).ToList())
            db.UsuarioRoles.Remove(ur);

        var actuales = usuario.UsuarioRoles.Select(x => x.RolID).ToHashSet();
        foreach (var rolId in rolIds.Where(r => !actuales.Contains(r)))
            usuario.UsuarioRoles.Add(new Usuario_Rol { RolID = rolId, FechaAsignacion = ahora });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<UsuarioDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el usuario. Verifica que el correo no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios, AccionesBitacora.Editar,
            "Usuario", id.ToString(), $"Edición del usuario {email}");

        return await ObtenerAsync(id, ct);
    }

    public async Task<ResultadoOperacion<bool>> CambiarEstadoAsync(
        int id, bool activo, int? actorId, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == id, ct);

        if (usuario is null)
            return ResultadoOperacion<bool>.Fallo(EstadoOperacion.NoEncontrado, "El usuario no existe.");

        if (usuario.Activo == activo)
            return ResultadoOperacion<bool>.Ok(true);

        if (!activo)
        {
            if (actorId == id)
                return ResultadoOperacion<bool>.Fallo(
                    EstadoOperacion.Conflicto, "No puedes desactivar tu propia cuenta.");

            var esAdmin = await db.UsuarioRoles
                .AnyAsync(ur => ur.UsuarioID == id && ur.Rol.Nombre == RolesSistema.Administrador, ct);

            if (esAdmin && !await HayOtroAdminActivoAsync(id, ct))
                return ResultadoOperacion<bool>.Fallo(
                    EstadoOperacion.Conflicto, "Debe quedar al menos un administrador activo.");
        }

        usuario.Activo = activo;
        usuario.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios,
            activo ? AccionesBitacora.Activar : AccionesBitacora.Desactivar,
            "Usuario", id.ToString(),
            $"{(activo ? "Activación" : "Desactivación")} del usuario {usuario.Email}");

        return ResultadoOperacion<bool>.Ok(true);
    }

    public async Task<ResultadoOperacion<ContrasenaTemporalDto>> RestablecerContrasenaAsync(
        int id, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.UsuarioID == id, ct);

        if (usuario is null)
            return ResultadoOperacion<ContrasenaTemporalDto>.Fallo(
                EstadoOperacion.NoEncontrado, "El usuario no existe.");

        var temporal = GenerarContrasenaTemporal();
        var (hash, salt) = hasher.Hashear(temporal);

        usuario.PasswordHash = hash;
        usuario.PasswordSalt = salt;
        usuario.DebeCambiarContrasena = true;
        usuario.IntentosFallidos = 0;
        usuario.FechaBloqueoHasta = null;
        usuario.FechaActualizacion = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios, AccionesBitacora.CambiarContrasena,
            "Usuario", id.ToString(), $"Restablecimiento de contraseña temporal de {usuario.Email}");

        return ResultadoOperacion<ContrasenaTemporalDto>.Ok(new ContrasenaTemporalDto
        {
            UsuarioID = usuario.UsuarioID,
            Email = usuario.Email,
            ContrasenaTemporal = temporal
        });
    }

    private async Task<string?> ValidarRolesAsync(List<int> rolIds, CancellationToken ct)
    {
        if (rolIds.Count == 0)
            return "Asigna al menos un rol.";

        var activos = await db.Roles.CountAsync(r => rolIds.Contains(r.RolID) && r.Activo, ct);

        return activos == rolIds.Count
            ? null
            : "Alguno de los roles no existe o está inactivo.";
    }

    private Task<int?> ObtenerRolAdminIdAsync(CancellationToken ct) =>
        db.Roles
            .Where(r => r.Nombre == RolesSistema.Administrador)
            .Select(r => (int?)r.RolID)
            .FirstOrDefaultAsync(ct);

    private Task<bool> HayOtroAdminActivoAsync(int usuarioId, CancellationToken ct) =>
        db.UsuarioRoles.AnyAsync(ur =>
            ur.UsuarioID != usuarioId &&
            ur.Rol.Nombre == RolesSistema.Administrador &&
            ur.Rol.Activo &&
            ur.Usuario.Activo, ct);

    // 16 caracteres con mayúscula, minúscula, número y símbolo (sin caracteres que se confundan al leer).
    private static string GenerarContrasenaTemporal()
    {
        const string mayusculas = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string minusculas = "abcdefghijkmnopqrstuvwxyz";
        const string numeros = "23456789";
        const string simbolos = "#$%&*?";
        const string todos = mayusculas + minusculas + numeros + simbolos;

        var caracteres = new List<char>
        {
            Elegir(mayusculas), Elegir(minusculas), Elegir(numeros), Elegir(simbolos)
        };

        while (caracteres.Count < 16)
            caracteres.Add(Elegir(todos));

        var arreglo = caracteres.ToArray();
        RandomNumberGenerator.Shuffle(arreglo.AsSpan());
        return new string(arreglo);
    }

    private static char Elegir(string origen) => origen[RandomNumberGenerator.GetInt32(origen.Length)];
}
