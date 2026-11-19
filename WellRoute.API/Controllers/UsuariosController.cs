using Microsoft.AspNetCore.Mvc;
using WellRoute.API.Authorization;
using WellRoute.Arquitecture.Auth;
using WellRoute.Arquitecture.Usuarios;
using WellRoute.Models.Dtos;

namespace WellRoute.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsuariosController : ControllerBase
	{
		private readonly IUsuarioService _usuarios;

		public UsuariosController(IUsuarioService usuarios)
		{
			_usuarios = usuarios;
		}

		[HttpGet]
		[RequierePermiso("user.read")]
		public async Task<IActionResult> Listar(CancellationToken ct)
		{
			return Ok(await _usuarios.ListarAsync(ct));
		}

		[HttpGet("{id:int}")]
		[RequierePermiso("user.read")]
		public async Task<IActionResult> Obtener(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _usuarios.ObtenerAsync(id, ct));
		}

		[HttpPost]
		[RequierePermiso("user.create")]
		public async Task<IActionResult> Crear([FromBody] GuardarUsuarioRequest solicitud, CancellationToken ct)
		{
			var resultado = await _usuarios.CrearAsync(solicitud, ct);

			if (resultado.Estado == EstadoOperacion.Exitoso)
			{
				return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.UsuarioID }, resultado.Datos);
			}

			return this.ToActionResult(resultado);
		}

		[HttpPut("{id:int}")]
		[RequierePermiso("user.update")]
		public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarUsuarioRequest solicitud, CancellationToken ct)
		{
			return this.ToActionResult(await _usuarios.ActualizarAsync(id, solicitud, ct));
		}

		[HttpPost("{id:int}/desactivar")]
		[RequierePermiso("user.deactivate")]
		public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _usuarios.CambiarEstadoAsync(id, false, User.ObtenerUsuarioId(), ct));
		}

		[HttpPost("{id:int}/activar")]
		[RequierePermiso("user.deactivate")]
		public async Task<IActionResult> Activar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _usuarios.CambiarEstadoAsync(id, true, User.ObtenerUsuarioId(), ct));
		}

		[HttpPost("{id:int}/restablecer-contrasena")]
		[RequierePermiso("user.update")]
		public async Task<IActionResult> RestablecerContrasena(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _usuarios.RestablecerContrasenaAsync(id, ct));
		}
	}
}
