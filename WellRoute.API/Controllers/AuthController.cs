using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Arquitecture.Auditoria;
using WellRoute.Arquitecture.Auth;
using WellRoute.Models.Dtos;

namespace WellRoute.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;
		private readonly IAuditoriaService _auditoria;

		public AuthController(IAuthService authService, IAuditoriaService auditoria)
		{
			_authService = authService;
			_auditoria = auditoria;
		}

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var resultado = await _authService.LoginAsync(request);

			return resultado.Resultado switch
			{
				ResultadoLogin.Exitoso => Ok(resultado),
				ResultadoLogin.CuentaBloqueada => StatusCode(StatusCodes.Status423Locked, resultado),
				ResultadoLogin.CuentaInactiva => StatusCode(StatusCodes.Status403Forbidden, resultado),
				_ => Unauthorized(resultado)
			};
		}

		// El token no se guarda en el servidor, así que cerrar sesión es que el Web lo descarte.
		// Aquí solo queda el registro del logout en Historial_Accesos (el usuario sale del token).
		[HttpPost("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			await _auditoria.RegistrarAccesoAsync(TipoAcceso.Logout);

			return NoContent();
		}

		[HttpGet("me")]
		[Authorize]
		public IActionResult Me()
		{
			return Ok(new
			{
				usuarioId = User.ObtenerUsuarioId(),
				email = User.ObtenerEmail(),
				nombre = User.ObtenerNombre(),
				roles = User.ObtenerRoles(),
				permisos = User.ObtenerPermisos(),
				debeCambiarContrasena = User.DebeCambiarContrasena()
			});
		}
        [HttpPost("cambiar-contrasena-temporal")]
        [Authorize(Policy = "CambioContrasenaTemporal")]
        public async Task<IActionResult> CambiarContrasenaTemporal(
			[FromBody] CambiarContrasenaTemporalRequest solicitud,
			CancellationToken ct)
        {
            var usuarioId = User.ObtenerUsuarioId();

            if (usuarioId is not int id || id <= 0)
                return Forbid();

            var estado = await _authService.CambiarContrasenaTemporalAsync(
                id, solicitud, ct);

            return estado switch
            {
                EstadoCambioContrasena.Exitoso => NoContent(),
                EstadoCambioContrasena.DatosInvalidos => BadRequest(),
                EstadoCambioContrasena.CuentaBloqueada =>
                    StatusCode(StatusCodes.Status423Locked),
                EstadoCambioContrasena.CambioNoPendiente => Conflict(),
                _ => Forbid()
            };
        }
    }
}