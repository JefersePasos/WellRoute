using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Arquitecture.Roles;
using WellRoute.Models.Dtos;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

// Gestión de destinos. La API es la que valida el permiso de cada acción.
[Authorize(Roles = RolesSistema.Administrador + ",Editor")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Route("Admin/Destinos/[action]/{id?}")]
public sealed class DestinosAdminController(AdminApiClient api) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ListarDestinosGestionAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            ViewBag.Error = respuesta.Mensaje ?? "No se pudo cargar la lista de destinos.";
            return View(new List<DestinoListaDto>());
        }

        return View(respuesta.Datos);
    }

    [HttpGet]
    public IActionResult Crear() => View("Formulario", new DestinoFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(DestinoFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        if (ModelState.IsValid)
        {
            var respuesta = await api.CrearDestinoAsync(token, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa)
            {
                TempData["Mensaje"] = "Destino creado.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo crear el destino.");
        }

        return View("Formulario", modelo);
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ObtenerDestinoGestionAsync(token, id, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cargar el destino.";
            return RedirectToAction(nameof(Index));
        }

        var d = respuesta.Datos;

        return View("Formulario", new DestinoFormViewModel
        {
            DestinoID = d.DestinoID,
            Nombre = d.Nombre,
            Pais = d.Pais,
            Categoria = d.Categoria,
            Descripcion = d.Descripcion,
            ImagenUrl = d.ImagenUrl,
            VideoUrl = d.VideoUrl
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, DestinoFormViewModel modelo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        modelo.DestinoID = id;

        if (ModelState.IsValid)
        {
            var respuesta = await api.ActualizarDestinoAsync(token, id, ComoSolicitud(modelo), ct);

            var redireccion = await ResolverSesionAsync(respuesta);
            if (redireccion is not null)
                return redireccion;

            if (respuesta.Exitosa)
            {
                TempData["Mensaje"] = "Destino actualizado.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError(string.Empty, respuesta.Mensaje ?? "No se pudo actualizar el destino.");
        }

        return View("Formulario", modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int id, bool activo, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.CambiarEstadoDestinoAsync(token, id, activo, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (respuesta.Exitosa)
            TempData["Mensaje"] = activo ? "Destino activado." : "Destino desactivado.";
        else
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo cambiar el estado del destino.";

        return RedirectToAction(nameof(Index));
    }

    private static GuardarDestinoRequest ComoSolicitud(DestinoFormViewModel modelo) => new()
    {
        Nombre = modelo.Nombre,
        Pais = modelo.Pais,
        Categoria = modelo.Categoria,
        Descripcion = modelo.Descripcion,
        ImagenUrl = modelo.ImagenUrl,
        VideoUrl = modelo.VideoUrl
    };
}
