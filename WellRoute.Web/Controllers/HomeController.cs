using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers
{
    public class HomeController(AdminApiClient api) : Controller
    {
        // Portada: collage con los destinos activos. Es pública; si hay sesión se marcan los favoritos.
        public async Task<IActionResult> Index(string? buscar, string? categoria, CancellationToken ct)
        {
            var modelo = new PortadaViewModel { Buscar = buscar, Categoria = categoria };

            var respuesta = await api.ListarDestinosPublicosAsync(buscar, categoria, ct);

            if (respuesta.Exitosa && respuesta.Datos is not null)
                modelo.Destinos = respuesta.Datos;
            else
                modelo.Error = "No se pudieron cargar los destinos. Intenta de nuevo en unos minutos.";

            if (User.Identity?.IsAuthenticated == true)
            {
                var token = await HttpContext.GetTokenAsync("access_token");

                if (!string.IsNullOrEmpty(token))
                {
                    var favoritos = await api.ListarIdsFavoritosAsync(token, ct);

                    if (favoritos.Exitosa && favoritos.Datos is not null)
                    {
                        modelo.FavoritosIds = favoritos.Datos.ToHashSet();
                        modelo.PuedeFavoritos = true;
                    }
                }
            }

            return View(modelo);
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
