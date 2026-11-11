using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Models.Dtos;
using WellRoute.Web.Models;
using WellRoute.Web.Services;

namespace WellRoute.Web.Controllers;

[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class CuentaController(AuthApiClient api) : Controller
{
    private const string TokenPendiente = "CambioContrasena.Token";

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Perfil));

        return View(new LoginRequest());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequest modelo, CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Perfil));

        await HttpContext.Session.LoadAsync(ct);
        HttpContext.Session.Remove(TokenPendiente);

        if (!ModelState.IsValid)
            return View(modelo);

        try
        {
            var (estado, datos) = await api.LoginAsync(modelo, ct);

            if (estado != HttpStatusCode.OK || datos is null ||
                datos.Resultado != ResultadoLogin.Exitoso ||
                string.IsNullOrWhiteSpace(datos.Token) ||
                datos.Expira is not DateTime expiracion)
            {
                return MostrarError(modelo, estado switch
                {
                    HttpStatusCode.BadRequest => "Revisa el correo y la contraseña.",
                    HttpStatusCode.Unauthorized => "Correo o contraseña incorrectos.",
                    HttpStatusCode.Forbidden => "La cuenta está desactivada.",
                    HttpStatusCode.Locked => "La cuenta está bloqueada temporalmente.",
                    _ => "No se pudo completar el inicio de sesión."
                });
            }

            var expiraUtc = new DateTimeOffset(expiracion.ToUniversalTime());
            if (expiraUtc <= DateTimeOffset.UtcNow)
                return MostrarError(modelo, "La respuesta de inicio de sesión ya venció.");

            if (datos.DebeCambiarContrasena)
            {
                // En este caso no emitimos la cookie de autenticación de la Web.
                HttpContext.Session.SetString(TokenPendiente, datos.Token);
                return RedirectToAction(nameof(CambiarContrasena));
            }

            // La API valida la firma del token antes de devolver la identidad.
            var perfil = await api.ObtenerPerfilAsync(datos.Token, ct);
            if (perfil?.UsuarioId is not int id || id <= 0 ||
                perfil.DebeCambiarContrasena ||
                string.IsNullOrWhiteSpace(perfil.Email))
            {
                return MostrarError(modelo, "No se pudo verificar la identidad en la API.");
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, id.ToString()),
                new(ClaimTypes.Email, perfil.Email),
                new(ClaimTypes.Name, perfil.Nombre ?? perfil.Email)
            };
            foreach (var rol in perfil.Roles ?? Array.Empty<string>())
                claims.Add(new Claim(ClaimTypes.Role, rol));

            var esquema = CookieAuthenticationDefaults.AuthenticationScheme;
            var identidad = new ClaimsIdentity(claims, esquema);

            await HttpContext.SignInAsync(esquema, new ClaimsPrincipal(identidad),
                new AuthenticationProperties { ExpiresUtc = expiraUtc, IsPersistent = false });

            return RedirectToAction(nameof(Perfil));
        }
        catch (Exception ex) when (!ct.IsCancellationRequested &&
            (ex is HttpRequestException or JsonException or OperationCanceledException))
        {
            return MostrarError(modelo,
                "No se pudo comunicar con la API de WellRoute. Verifica que esté iniciada.");
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> CambiarContrasena(CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Perfil));

        await HttpContext.Session.LoadAsync(ct);
        if (string.IsNullOrEmpty(HttpContext.Session.GetString(TokenPendiente)))
            return RedirectToAction(nameof(Login));

        return View(new CambiarContrasenaViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarContrasena(
        CambiarContrasenaViewModel modelo, CancellationToken ct)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction(nameof(Perfil));

        await HttpContext.Session.LoadAsync(ct);
        var token = HttpContext.Session.GetString(TokenPendiente);
        if (string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(Login));

        if (!ModelState.IsValid)
            return View(modelo);

        if (string.Equals(modelo.ContrasenaActual, modelo.ContrasenaNueva,
            StringComparison.Ordinal))
        {
            ModelState.AddModelError(nameof(modelo.ContrasenaNueva),
                "La contraseña nueva debe ser distinta de la temporal.");
            return View(modelo);
        }

        try
        {
            var estado = await api.CambiarContrasenaAsync(
                token, modelo.ContrasenaActual, modelo.ContrasenaNueva, ct);

            if (estado == HttpStatusCode.NoContent)
            {
                HttpContext.Session.Remove(TokenPendiente);
                TempData["MensajeCuenta"] = "Contraseña actualizada. Inicia sesión con la nueva.";
                return RedirectToAction(nameof(Login));
            }

            if (estado is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or
                HttpStatusCode.Conflict or HttpStatusCode.Locked)
            {
                HttpContext.Session.Remove(TokenPendiente);
                TempData["MensajeCuenta"] = estado == HttpStatusCode.Locked
                    ? "La cuenta está bloqueada temporalmente. Intenta más tarde."
                    : "El cambio ya no está disponible en esta sesión. Inicia sesión de nuevo.";
                return RedirectToAction(nameof(Login));
            }

            return MostrarError(modelo, estado switch
            {
                HttpStatusCode.BadRequest => "Revisa la contraseña temporal y los datos ingresados.",
                HttpStatusCode.NotFound => "La API todavía no tiene habilitado el cambio de contraseña.",
                _ => "No se pudo guardar la contraseña. Intenta de nuevo."
            });
        }
        catch (Exception ex) when (!ct.IsCancellationRequested &&
            (ex is HttpRequestException or JsonException or OperationCanceledException))
        {
            return MostrarError(modelo,
                "No se pudo comunicar con la API de WellRoute.");
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult Perfil() => View();

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CerrarSesion(CancellationToken ct)
    {
        await HttpContext.Session.LoadAsync(ct);
        HttpContext.Session.Remove(TokenPendiente);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult MostrarError(object modelo, string mensaje)
    {
        ModelState.AddModelError(string.Empty, mensaje);
        return View(modelo);
    }
}
