using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Models.Dtos;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

// Mis favoritos. Cualquier usuario con sesión iniciada; la API valida el permiso.
[Authorize]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class FavoritosController(AdminApiClient api) : AdminControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = await api.ListarFavoritosAsync(token, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            ViewBag.Error = respuesta.Mensaje ?? "No se pudieron cargar tus favoritos.";
            return View(new List<DestinoListaDto>());
        }

        return View(respuesta.Datos);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Alternar(int destinoId, bool agregar, string? returnUrl, CancellationToken ct)
    {
        var token = await ObtenerTokenAsync();
        if (string.IsNullOrEmpty(token))
            return Challenge();

        var respuesta = agregar
            ? await api.AgregarFavoritoAsync(token, destinoId, ct)
            : await api.QuitarFavoritoAsync(token, destinoId, ct);

        var redireccion = await ResolverSesionAsync(respuesta);
        if (redireccion is not null)
            return redireccion;

        if (!respuesta.Exitosa)
            TempData["Error"] = respuesta.Mensaje ?? "No se pudo actualizar tus favoritos.";

        // Solo se vuelve a páginas de este mismo sitio.
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
