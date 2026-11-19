using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Arquitecture.Roles;
using WellRoute.Models.Dtos;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

[Authorize(Roles = RolesSistema.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class RolesController(AdminApiClient api) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ListarRolesAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            ViewBag.Error = respuesta.Mensaje ?? "No se pudo cargar la lista de roles.";
            return View(new List<RolListaDto>());
        }

        return View(respuesta.Datos);
    }

    [HttpGet]
    public async Task<IActionResult> Crear(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var modelo = new RolFormViewModel();

        var redireccion = await CargarPermisosAsync(modelo, token, ct);
        if (redireccion is not null)
            return redireccion;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(RolFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        if (ModelState.IsValid)
        {
            var respuesta = await api.CrearRolAsync(token, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa)
            {
                TempData["Mensaje"] = "Rol creado.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo crear el rol.");
        }

        var errorPermisos = await CargarPermisosAsync(modelo, token, ct);
        if (errorPermisos is not null)
            return errorPermisos;

        return View("Formulario", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ObtenerRolAsync(token, id, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cargar el rol.";
            return RedirectToAction(nameof(Index));
        }

        var modelo = new RolFormViewModel
        {
            RolID = respuesta.Datos.RolID,
            Nombre = respuesta.Datos.Nombre,
            Descripcion = respuesta.Datos.Descripcion,
            PermisoIds = respuesta.Datos.PermisoIds,
            EsAdministrador = respuesta.Datos.Nombre == RolesSistema.Administrador
        };

        var errorPermisos = await CargarPermisosAsync(modelo, token, ct);
        if (errorPermisos is not null)
            return errorPermisos;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, RolFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        modelo.RolID = id;

        if (ModelState.IsValid)
        {
            var respuesta = await api.ActualizarRolAsync(token, id, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa)
            {
                TempData["Mensaje"] = "Rol actualizado.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo actualizar el rol.");
        }

        var errorPermisos = await CargarPermisosAsync(modelo, token, ct);
        if (errorPermisos is not null)
            return errorPermisos;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool activo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.CambiarEstadoRolAsync(token, id, activo, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (respuesta.Exitosa)
            TempData["Mensaje"] = activo ? "Rol activado." : "Rol desactivado.";
        else
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cambiar el estado del rol.";

        return RedirectToAction(nameof(Index));
    }

    private static GuardarRolRequest ComoSolicitud(RolFormViewModel modelo) => new()
    {
        Nombre = modelo.Nombre,
        Descripcion = modelo.Descripcion,
        PermisoIds = modelo.PermisoIds
    };

    private async Task<IActionResult?> CargarPermisosAsync(
        RolFormViewModel modelo, string token, CancellationToken ct)
    {
        var respuesta = await api.ListarPermisosAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (respuesta.Exitosa && respuesta.Datos is not null)
            modelo.PermisosDisponibles = respuesta.Datos;
        else
            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudieron cargar los permisos.");

        return null;
    }
}
