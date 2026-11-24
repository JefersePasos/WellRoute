using Microsoft.AspNetCore.Mvc;
using WellRoute.API.Authorization;
using WellRoute.Arquitecture.Destinos;
using WellRoute.Models.Dtos;

namespace WellRoute.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class DestinosController : ControllerBase
	{
		private readonly IDestinoService _destinos;

		public DestinosController(IDestinoService destinos)
		{
			_destinos = destinos;
		}

		// ---------- Público: portada y ficha (solo destinos activos) ----------

		[HttpGet]
		public async Task<IActionResult> ListarPublicos(
			[FromQuery] string? buscar, [FromQuery] string? categoria, CancellationToken ct)
		{
			return Ok(await _destinos.ListarPublicosAsync(buscar, categoria, ct));
		}

		[HttpGet("{id:int}")]
		public async Task<IActionResult> ObtenerPublico(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _destinos.ObtenerPublicoAsync(id, ct));
		}

		// ---------- Gestión (incluye inactivos) ----------

		[HttpGet("gestion")]
		[RequierePermiso("destino.update")]
		public async Task<IActionResult> ListarGestion(CancellationToken ct)
		{
			return Ok(await _destinos.ListarGestionAsync(ct));
		}

		[HttpGet("gestion/{id:int}")]
		[RequierePermiso("destino.update")]
		public async Task<IActionResult> ObtenerGestion(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _destinos.ObtenerGestionAsync(id, ct));
		}

		[HttpPost]
		[RequierePermiso("destino.create")]
		public async Task<IActionResult> Crear([FromBody] GuardarDestinoRequest solicitud, CancellationToken ct)
		{
			var resultado = await _destinos.CrearAsync(solicitud, ct);

			if (resultado.Estado == EstadoOperacion.Exitoso)
			{
				return CreatedAtAction(nameof(ObtenerGestion), new { id = resultado.Datos!.DestinoID }, resultado.Datos);
			}

			return this.ToActionResult(resultado);
		}

		[HttpPut("{id:int}")]
		[RequierePermiso("destino.update")]
		public async Task<IActionResult> Actualizar(int id, [FromBody] GuardarDestinoRequest solicitud, CancellationToken ct)
		{
			return this.ToActionResult(await _destinos.ActualizarAsync(id, solicitud, ct));
		}

		[HttpPost("{id:int}/desactivar")]
		[RequierePermiso("destino.deactivate")]
		public async Task<IActionResult> Desactivar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _destinos.CambiarEstadoAsync(id, false, ct));
		}

		[HttpPost("{id:int}/activar")]
		[RequierePermiso("destino.deactivate")]
		public async Task<IActionResult> Activar(int id, CancellationToken ct)
		{
			return this.ToActionResult(await _destinos.CambiarEstadoAsync(id, true, ct));
		}
	}
}
