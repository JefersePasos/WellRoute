using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Arquitecture.Roles;
using WellRoute.Models.Dtos;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

[Authorize(Roles = RolesSistema.Administrador)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class UsuariosController(AdminApiClient api) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ListarUsuariosAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            ViewBag.Error = respuesta.Mensaje ?? "No se pudo cargar la lista de usuarios.";
            return View(new List<UsuarioListaDto>());
        }

        return View(respuesta.Datos);
    }

    [HttpGet]
    public async Task<IActionResult> Crear(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var modelo = new UsuarioFormViewModel();

        var redireccion = await CargarRolesAsync(modelo, token, ct);
        if (redireccion is not null)
            return redireccion;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(UsuarioFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        if (ModelState.IsValid)
        {
            var respuesta = await api.CrearUsuarioAsync(token, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa && respuesta.Datos is not null)
            {
                // La contraseña temporal se muestra solo en esta respuesta, no se guarda en ningún lado.
                return View("ContrasenaTemporal", new ContrasenaTemporalViewModel
                {
                    Titulo = "Usuario creado",
                    Email = respuesta.Datos.Email,
                    Contrasena = respuesta.Datos.ContrasenaTemporal
                });
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo crear el usuario.");
        }

        var errorRoles = await CargarRolesAsync(modelo, token, ct);
        if (errorRoles is not null)
            return errorRoles;

        return View("Formulario", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ObtenerUsuarioAsync(token, id, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cargar el usuario.";
            return RedirectToAction(nameof(Index));
        }

        var modelo = new UsuarioFormViewModel
        {
            UsuarioID = respuesta.Datos.UsuarioID,
            Nombre = respuesta.Datos.Nombre,
            Email = respuesta.Datos.Email,
            Telefono = respuesta.Datos.Telefono,
            RolIds = respuesta.Datos.RolIds
        };

        var errorRoles = await CargarRolesAsync(modelo, token, ct);
        if (errorRoles is not null)
            return errorRoles;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        modelo.UsuarioID = id;

        if (ModelState.IsValid)
        {
            var respuesta = await api.ActualizarUsuarioAsync(token, id, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa)
            {
                TempData["Mensaje"] = "Usuario actualizado.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo actualizar el usuario.");
        }

        var errorRoles = await CargarRolesAsync(modelo, token, ct);
        if (errorRoles is not null)
            return errorRoles;

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool activo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.CambiarEstadoUsuarioAsync(token, id, activo, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (respuesta.Exitosa)
            TempData["Mensaje"] = activo ? "Usuario activado." : "Usuario desactivado.";
        else
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cambiar el estado del usuario.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RestablecerContrasena(int id, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.RestablecerContrasenaAsync(token, id, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo restablecer la contraseña.";
            return RedirectToAction(nameof(Index));
        }

        return View("ContrasenaTemporal", new ContrasenaTemporalViewModel
        {
            Titulo = "Contraseña restablecida",
            Email = respuesta.Datos.Email,
            Contrasena = respuesta.Datos.ContrasenaTemporal
        });
    }

    private static GuardarUsuarioRequest ComoSolicitud(UsuarioFormViewModel modelo) => new()
    {
        Nombre = modelo.Nombre,
        Email = modelo.Email,
        Telefono = modelo.Telefono,
        RolIds = modelo.RolIds
    };

    // Carga los roles activos para mostrarlos como opciones. Devuelve una redirección si la sesión no sirve.
    private async Task<IActionResult?> CargarRolesAsync(
        UsuarioFormViewModel modelo, string token, CancellationToken ct)
    {
        var respuesta = await api.ListarRolesAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (respuesta.Exitosa && respuesta.Datos is not null)
            modelo.RolesDisponibles = respuesta.Datos.Where(r => r.Activo).ToList();
        else
            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudieron cargar los roles.");

        return null;
    }
}
