using Microsoft.AspNetCore.Mvc;
using WellRoute.API.Authorization;
using WellRoute.Arquitecture.Roles;
using WellRoute.Models.Dtos;

namespace WellRoute.API.Controllers
{
	// La gestión de roles es del panel administrativo, por eso pide admin.access.
	[ApiController]
	[Route("api/[controller]")]
	[RequierePermiso(RolesSistema.PermisoAdmin)]
	public class RolesController : ControllerBase
	{
		private readonly IRolService _roles;

		public RolesController(IRolService roles)
		{
			_roles = roles;
		}

		[HttpGet]
		public async Task<IActionResult> Listar(CancellationToken ct)
		{
			return Ok(await _roles.ListarAsync(ct));
		}

		[HttpGet("permisos")]
		public async Task<IActionResult> ListarPermisos(CancellationToken ct)
		{
			return Ok(await _roles.ListarPermisosAsync(ct));
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> Obtener(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _roles.ObtenerAsync(id, ct));
		}

		[HttpPost]
		public async Task<IActionResult> Crear([FromBody] GuardarRolRequest solicitud, CancellationToken ct)
		{
			var resultado = await _roles.CrearAsync(solicitud, ct);

			if (resultado.Estado == EstadoOperacion.Exitoso)
			{
				return CreatedAtAction(nameof(Obtener), new { id = resultado.Datos!.RolID }, resultado.Datos);
			}

			return this.ToActionResult(resultado);
		}

		[HttpPut("{id:int}")]
		public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarRolRequest solicitud, CancellationToken ct)
		{
			return this.ToActionResult(await _roles.ActualizarAsync(id, solicitud, ct));
		}

		[HttpPost("{id:int}/desactivar")]
		public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _roles.CambiarEstadoAsync(id, false, ct));
		}

		[HttpPost("{id:int}/activar")]
		public async Task<IActionResult> Activar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _roles.CambiarEstadoAsync(id, true, ct));
		}
	}
}
