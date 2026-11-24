using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WellRoute.Models.Dtos;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers
{
    public class HomeController(AdminApiClient api) : Controller
    {
        // Portada: collage con los destinos activos. Es pública.
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var respuesta = await api.ListarDestinosPublicosAsync(ct);

            if (!respuesta.Exitosa || respuesta.Datos is null)
            {
                ViewBag.Error = "No se pudieron cargar los destinos. Intenta de nuevo en unos minutos.";
                return View(new List<DestinoListaDto>());
            }

            return View(respuesta.Datos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
