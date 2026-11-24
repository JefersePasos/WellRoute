using Microsoft.AspNetCore.Mvc;
using WellRoute.API.Authorization;
using WellRoute.Arquitecture.Auth;
using WellRoute.Arquitecture.Favoritos;

namespace WellRoute.API.Controllers
{
	// Cada usuario solo ve y cambia sus propios favoritos: el usuario sale del token, nunca de la URL.
	[ApiController]
	[Route("api/[controller]")]
	[RequierePermiso("destino.read")]
	public class FavoritosController : ControllerBase
	{
		private readonly IFavoritoService _favoritos;

		public FavoritosController(IFavoritoService favoritos)
		{
			_favoritos = favoritos;
		}

		[HttpGet]
		public async Task<IActionResult> Listar(CancellationToken ct)
		{
			if (User.ObtenerUsuarioId() is not int usuarioId)
			{
				return Unauthorized();
			}

			return Ok(await _favoritos.ListarAsync(usuarioId, ct));
		}

		[HttpGet("ids")]
		public async Task<IActionResult> ListarIds(CancellationToken ct)
		{
			if (User.ObtenerUsuarioId() is not int usuarioId)
			{
				return Unauthorized();
			}

			return Ok(await _favoritos.ListarIdsAsync(usuarioId, ct));
		}

		[HttpPost("{destinoId:int}")]
		public async Task<IActionResult> Agregar(int destinoId, CancellationToken ct)
		{
			if (User.ObtenerUsuarioId() is not int usuarioId)
			{
				return Unauthorized();
			}

			return this.ToActionResult(await _favoritos.AgregarAsync(usuarioId, destinoId, ct));
		}

		[HttpDelete("{destinoId:int}")]
		public async Task<IActionResult> Quitar(int destinoId, CancellationToken ct)
		{
			if (User.ObtenerUsuarioId() is not int usuarioId)
			{
				return Unauthorized();
			}

			return this.ToActionResult(await _favoritos.QuitarAsync(usuarioId, destinoId, ct));
		}
	}
}
