using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using WellRoute.Models.Dtos;

namespace WellRoute.Web.Services;

public sealed class AuthApiClient(HttpClient http)
{
    public async Task<(HttpStatusCode Estado, LoginResult? Datos)> LoginAsync(
        LoginRequest solicitud, CancellationToken ct)
    {
        using var respuesta = await http.PostAsJsonAsync("api/auth/login", solicitud, ct);

        // En error usamos el estado HTTP, sin mostrar al usuario el cuerpo de la API.
        var datos = respuesta.IsSuccessStatusCode
            ? await respuesta.Content.ReadFromJsonAsync<LoginResult>(cancellationToken: ct)
            : null;

        return (respuesta.StatusCode, datos);
    }

    public async Task<PerfilApi?> ObtenerPerfilAsync(string token, CancellationToken ct)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Get, "api/auth/me");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        using var respuesta = await http.SendAsync(solicitud, ct);

        return respuesta.IsSuccessStatusCode
            ? await respuesta.Content.ReadFromJsonAsync<PerfilApi>(cancellationToken: ct)
            : null;
    }

    public async Task<HttpStatusCode> CambiarContrasenaAsync(
        string token, string actual, string nueva, CancellationToken ct)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Post,
            "api/auth/cambiar-contrasena-temporal");

        // El token pertenece a esta petición. No lo ponemos en DefaultRequestHeaders.
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        solicitud.Content = JsonContent.Create(new
        {
            ContrasenaActual = actual,
            ContrasenaNueva = nueva
        });

        using var respuesta = await http.SendAsync(solicitud, ct);
        return respuesta.StatusCode;
    }

    // Avisa a la API para que registre el logout en Historial_Accesos.
    public async Task CerrarSesionAsync(string token, CancellationToken ct)
    {
        using var solicitud = new HttpRequestMessage(HttpMethod.Post, "api/auth/logout");
        solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        using var respuesta = await http.SendAsync(solicitud, ct);
    }
}

public sealed class PerfilApi
{
    public int? UsuarioId { get; init; }
    public string? Email { get; init; }
    public string? Nombre { get; init; }
    public string[]? Roles { get; init; }
    public bool DebeCambiarContrasena { get; init; }
}