using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using WellRoute.Models.Dtos;

namespace WellRoute.Web.Services;

// Llamadas a la API para gestionar usuarios y roles. El token va en cada petición.
public sealed class AdminApiClient(HttpClient http)
{
    // ---------- Usuarios ----------
    public Task<RespuestaApi<List<UsuarioListaDto>>> ListarUsuariosAsync(string token, CancellationToken ct) =>
        EnviarAsync<List<UsuarioListaDto>>(HttpMethod.Get, "api/usuarios", token, null, ct);

    public Task<RespuestaApi<UsuarioDetalleDto>> ObtenerUsuarioAsync(string token, int id, CancellationToken ct) =>
        EnviarAsync<UsuarioDetalleDto>(HttpMethod.Get, $"api/usuarios/{id}", token, null, ct);

    public Task<RespuestaApi<ContrasenaTemporalDto>> CrearUsuarioAsync(
        string token, GuardarUsuarioRequest solicitud, CancellationToken ct) =>
        EnviarAsync<ContrasenaTemporalDto>(HttpMethod.Post, "api/usuarios", token, solicitud, ct);

    public Task<RespuestaApi<UsuarioDetalleDto>> ActualizarUsuarioAsync(
        string token, int id, GuardarUsuarioRequest solicitud, CancellationToken ct) =>
        EnviarAsync<UsuarioDetalleDto>(HttpMethod.Put, $"api/usuarios/{id}", token, solicitud, ct);

    public Task<RespuestaApi<bool>> CambiarEstadoUsuarioAsync(
        string token, int id, bool activo, CancellationToken ct) =>
        EnviarAsync<bool>(HttpMethod.Post,
            $"api/usuarios/{id}/{(activo ? "activar" : "desactivar")}", token, null, ct);

    public Task<RespuestaApi<ContrasenaTemporalDto>> RestablecerContrasenaAsync(
        string token, int id, CancellationToken ct) =>
        EnviarAsync<ContrasenaTemporalDto>(HttpMethod.Post,
            $"api/usuarios/{id}/restablecer-contrasena", token, null, ct);

    // ---------- Roles ----------
    public Task<RespuestaApi<List<RolListaDto>>> ListarRolesAsync(string token, CancellationToken ct) =>
        EnviarAsync<List<RolListaDto>>(HttpMethod.Get, "api/roles", token, null, ct);

    public Task<RespuestaApi<List<PermisoDto>>> ListarPermisosAsync(string token, CancellationToken ct) =>
        EnviarAsync<List<PermisoDto>>(HttpMethod.Get, "api/roles/permisos", token, null, ct);

    public Task<RespuestaApi<RolDetalleDto>> ObtenerRolAsync(string token, int id, CancellationToken ct) =>
        EnviarAsync<RolDetalleDto>(HttpMethod.Get, $"api/roles/{id}", token, null, ct);

    public Task<RespuestaApi<RolDetalleDto>> CrearRolAsync(
        string token, GuardarRolRequest solicitud, CancellationToken ct) =>
        EnviarAsync<RolDetalleDto>(HttpMethod.Post, "api/roles", token, solicitud, ct);

    public Task<RespuestaApi<RolDetalleDto>> ActualizarRolAsync(
        string token, int id, GuardarRolRequest solicitud, CancellationToken ct) =>
        EnviarAsync<RolDetalleDto>(HttpMethod.Put, $"api/roles/{id}", token, solicitud, ct);

    public Task<RespuestaApi<bool>> CambiarEstadoRolAsync(
        string token, int id, bool activo, CancellationToken ct) =>
        EnviarAsync<bool>(HttpMethod.Post,
            $"api/roles/{id}/{(activo ? "activar" : "desactivar")}", token, null, ct);

    private async Task<RespuestaApi<T>> EnviarAsync<T>(
        HttpMethod metodo, string ruta, string token, object? cuerpo, CancellationToken ct)
    {
        try
        {
            using var solicitud = new HttpRequestMessage(metodo, ruta);

            // El token pertenece a esta petición. No lo ponemos en DefaultRequestHeaders.
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            if (cuerpo is not null)
                solicitud.Content = JsonContent.Create(cuerpo, cuerpo.GetType());

            using var respuesta = await http.SendAsync(solicitud, ct);

            if (respuesta.IsSuccessStatusCode)
            {
                var datos = await respuesta.Content.ReadFromJsonAsync<T>(cancellationToken: ct);
                return new RespuestaApi<T> { Estado = respuesta.StatusCode, Datos = datos };
            }

            // Solo se lee el mensaje que arma la API para conflictos y datos inválidos.
            string? mensaje = null;
            try
            {
                var error = await respuesta.Content.ReadFromJsonAsync<ErrorApi>(cancellationToken: ct);
                mensaje = error?.Mensaje;
            }
            catch (Exception ex) when (ex is JsonException or NotSupportedException)
            {
            }

            return new RespuestaApi<T> { Estado = respuesta.StatusCode, Mensaje = mensaje };
        }
        catch (Exception ex) when (!ct.IsCancellationRequested &&
            (ex is HttpRequestException or TaskCanceledException or JsonException))
        {
            return new RespuestaApi<T>
            {
                Estado = HttpStatusCode.ServiceUnavailable,
                Mensaje = "No se pudo comunicar con la API de WellRoute."
            };
        }
    }

    private sealed class ErrorApi
    {
        public string? Mensaje { get; set; }
    }
}

public sealed class RespuestaApi<T>
{
    public HttpStatusCode Estado { get; init; }

    public T? Datos { get; init; }

    public string? Mensaje { get; init; }

    public bool Exitosa => (int)Estado is >= 200 and < 300;
}
