using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

// Base de los controladores del panel administrativo.
public abstract class AdminControllerBase : Controller
{
    private const string TokenApi = "access_token";

    protected Task<string?> ObtenerTokenAsync() => HttpContext.GetTokenAsync(TokenApi);

    // Devuelve una redirección si la sesión venció o no hay permiso. Si todo está bien devuelve null.
    protected async Task<IActionResult?> ResolverSesionAsync<T>(RespuestaApi<T> respuesta)
    {
        if (respuesta.Estado == HttpStatusCode.Unauthorized)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Cuenta");
        }

        if (respuesta.Estado == HttpStatusCode.Forbidden)
        {
            TempData["Error"] = "No tienes permiso para realizar esta acción.";
            return RedirectToAction("Index", "Home");
        }

        return null;
    }
}
