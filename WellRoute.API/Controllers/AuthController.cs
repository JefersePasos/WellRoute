using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WellRoute.Arquitecture.Auth;
using WellRoute.Models.Dtos;

namespace WellRoute.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
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

		// El token no se guarda en el servidor, así que cerrar sesión es que el Web lo descarte
		[HttpPost("logout")]
		[Authorize]
		public IActionResult Logout()
		{
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
	}
}