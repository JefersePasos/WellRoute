using Microsoft.EntityFrameworkCore;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Dtos;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Roles;

public interface IRolService
{
    Task<List<RolListaDto>> ListarAsync(CancellationToken ct);

    Task<List<PermisoDto>> ListarPermisosAsync(CancellationToken ct);

    Task<ResultadoOperacion<RolDetalleDto>> ObtenerAsync(int id, CancellationToken ct);

    Task<ResultadoOperacion<RolDetalleDto>> CrearAsync(GuardarRolRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<RolDetalleDto>> ActualizarAsync(int id, GuardarRolRequest solicitud, CancellationToken ct);

    Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct);
}

public class RolService(WellRouteDbContext db, IAuditoriaService auditoria) : IRolService
{
    public async Task<List<RolListaDto>> ListarAsync(CancellationToken ct)
    {
        return await db.Roles
            .AsNoTracking()
            .OrderBy(r => r.Nombre)
            .Select(r => new RolListaDto
            {
                RolID = r.RolID,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Activo = r.Activo,
                CantidadUsuarios = r.UsuarioRoles.Count,
                CantidadPermisos = r.RolPermisos.Count
            })
            .ToListAsync(ct);
    }

    public async Task<List<PermisoDto>> ListarPermisosAsync(CancellationToken ct)
    {
        return await db.Permisos
            .AsNoTracking()
            .OrderBy(p => p.Codigo)
            .Select(p => new PermisoDto
            {
                PermisoID = p.PermisoID,
                Nombre = p.Nombre,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion
            })
            .ToListAsync(ct);
    }

    public async Task<ResultadoOperacion<RolDetalleDto>> ObtenerAsync(int id, CancellationToken ct)
    {
        var rol = await db.Roles
            .AsNoTracking()
            .Where(r => r.RolID == id)
            .Select(r => new RolDetalleDto
            {
                RolID = r.RolID,
                Nombre = r.Nombre,
                Descripcion = r.Descripcion,
                Activo = r.Activo,
                PermisoIds = r.RolPermisos.Select(rp => rp.PermisoID).ToList()
            })
            .FirstOrDefaultAsync(ct);

        return rol is null
            ? ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El rol no existe.")
            : ResultadoOperacion<RolDetalleDto>.Ok(rol);
    }

    public async Task<ResultadoOperacion<RolDetalleDto>> CrearAsync(GuardarRolRequest solicitud, CancellationToken ct)
    {
        var nombre = solicitud.Nombre.Trim();
        var permisoIds = solicitud.PermisoIds.Distinct().ToList();

        if (!await PermisosExistenAsync(permisoIds, ct))
            return ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.Invalido, "Alguno de los permisos no existe.");

        if (await db.Roles.AnyAsync(r => r.Nombre == nombre, ct))
            return ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.Conflicto, "Ya existe un rol con ese nombre.");

        var ahora = DateTime.UtcNow;

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = solicitud.Descripcion?.Trim() ?? string.Empty,
            Activo = true,
            FechaCreacion = ahora
        };

        foreach (var permisoId in permisoIds)
            rol.RolPermisos.Add(new Rol_Permiso { PermisoID = permisoId, FechaAsignacion = ahora });

        db.Roles.Add(rol);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<RolDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el rol. Verifica que el nombre no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios, AccionesBitacora.Crear,
            "Rol", rol.RolID.ToString(), $"Alta del rol {nombre}");

        return await ObtenerAsync(rol.RolID, ct);
    }

    public async Task<ResultadoOperacion<RolDetalleDto>> ActualizarAsync(
        int id, GuardarRolRequest solicitud, CancellationToken ct)
    {
        var rol = await db.Roles
            .Include(r => r.RolPermisos)
            .FirstOrDefaultAsync(r => r.RolID == id, ct);

        if (rol is null)
            return ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.NoEncontrado, "El rol no existe.");

        if (rol.Nombre == RolesSistema.Administrador)
            return ResultadoOperacion<RolDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "El rol Administrador no se puede modificar.");

        var nombre = solicitud.Nombre.Trim();
        var permisoIds = solicitud.PermisoIds.Distinct().ToList();

        if (!await PermisosExistenAsync(permisoIds, ct))
            return ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.Invalido, "Alguno de los permisos no existe.");

        if (await db.Roles.AnyAsync(r => r.Nombre == nombre && r.RolID != id, ct))
            return ResultadoOperacion<RolDetalleDto>.Fallo(EstadoOperacion.Conflicto, "Ya existe otro rol con ese nombre.");

        var ahora = DateTime.UtcNow;

        rol.Nombre = nombre;
        rol.Descripcion = solicitud.Descripcion?.Trim() ?? string.Empty;

        foreach (var rp in rol.RolPermisos.Where(x => !permisoIds.Contains(x.PermisoID)).ToList())
            db.RolPermisos.Remove(rp);

        var actuales = rol.RolPermisos.Select(x => x.PermisoID).ToHashSet();
        foreach (var permisoId in permisoIds.Where(p => !actuales.Contains(p)))
            rol.RolPermisos.Add(new Rol_Permiso { PermisoID = permisoId, FechaAsignacion = ahora });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return ResultadoOperacion<RolDetalleDto>.Fallo(
                EstadoOperacion.Conflicto, "No se pudo guardar el rol. Verifica que el nombre no esté repetido.");
        }

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios, AccionesBitacora.Editar,
            "Rol", id.ToString(), $"Edición del rol {nombre}");

        return await ObtenerAsync(id, ct);
    }

    public async Task<ResultadoOperacion<bool>> CambiarEstadoAsync(int id, bool activo, CancellationToken ct)
    {
        var rol = await db.Roles.FirstOrDefaultAsync(r => r.RolID == id, ct);

        if (rol is null)
            return ResultadoOperacion<bool>.Fallo(EstadoOperacion.NoEncontrado, "El rol no existe.");

        if (rol.Nombre == RolesSistema.Administrador && !activo)
            return ResultadoOperacion<bool>.Fallo(
                EstadoOperacion.Conflicto, "El rol Administrador no se puede desactivar.");

        if (rol.Activo == activo)
            return ResultadoOperacion<bool>.Ok(true);

        rol.Activo = activo;
        await db.SaveChangesAsync(ct);

        await auditoria.RegistrarAsync(
            ModulosBitacora.Usuarios,
            activo ? AccionesBitacora.Activar : AccionesBitacora.Desactivar,
            "Rol", id.ToString(),
            $"{(activo ? "Activación" : "Desactivación")} del rol {rol.Nombre}");

        return ResultadoOperacion<bool>.Ok(true);
    }

    private async Task<bool> PermisosExistenAsync(List<int> permisoIds, CancellationToken ct)
    {
        if (permisoIds.Count == 0)
            return true;

        return await db.Permisos.CountAsync(p => permisoIds.Contains(p.PermisoID), ct) == permisoIds.Count;
    }
}
