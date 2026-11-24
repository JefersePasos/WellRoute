using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

// Ficha pública de un destino.
public sealed class DestinosController(AdminApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Detalle(int id, CancellationToken ct)
    {
        var respuesta = await api.ObtenerDestinoPublicoAsync(id, ct);

        if (!respuesta.Exitosa || respuesta.Datos is null)
        {
            TempData["Error"] = respuesta.Estado == HttpStatusCode.NotFound
                ? "Ese destino no existe o ya no está disponible."
                : "No se pudo cargar el destino. Intenta de nuevo.";
            return RedirectToAction("Index", "Home");
        }

        var puedeFavoritos = false;
        var esFavorito = false;

        if (User.Identity?.IsAuthenticated == true)
        {
            var token = await HttpContext.GetTokenAsync("access_token");

            if (!string.IsNullOrEmpty(token))
            {
                var favoritos = await api.ListarIdsFavoritosAsync(token, ct);

                if (favoritos.Exitosa && favoritos.Datos is not null)
                {
                    puedeFavoritos = true;
                    esFavorito = favoritos.Datos.Contains(id);
                }
            }
        }

        return View(new DestinoFichaViewModel
        {
            Destino = respuesta.Datos,
            PuedeFavoritos = puedeFavoritos,
            EsFavorito = esFavorito
        });
    }
}
