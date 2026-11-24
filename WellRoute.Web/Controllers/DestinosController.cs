using Microsoft.AspNetCore.Mvc;
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
            TempData["Error"] = respuesta.Estado == System.Net.HttpStatusCode.NotFound
                ? "Ese destino no existe o ya no está disponible."
                : "No se pudo cargar el destino. Intenta de nuevo.";
            return RedirectToAction("Index", "Home");
        }

        return View(respuesta.Datos);
    }
}
