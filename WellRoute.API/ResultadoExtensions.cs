using Microsoft.AspNetCore.Mvc;
using WellRoute.Models.Dtos;

namespace WellRoute.API
{
	public static class ResultadoExtensions
	{
		// Convierte el resultado de un servicio en la respuesta HTTP que corresponde.
		public static IActionResult ToActionResult<T>(this ControllerBase controlador, ResultadoOperacion<T> resultado)
		{
			return resultado.Estado switch
			{
				EstadoOperacion.Exitoso => controlador.Ok(resultado.Datos),
				EstadoOperacion.NoEncontrado => controlador.NotFound(new { mensaje = resultado.Mensaje }),
				EstadoOperacion.Conflicto => controlador.Conflict(new { mensaje = resultado.Mensaje }),
				_ => controlador.BadRequest(new { mensaje = resultado.Mensaje })
			};
		}
	}
}
