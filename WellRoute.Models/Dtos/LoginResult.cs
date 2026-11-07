namespace WellRoute.Models.Dtos
{
	public class LoginResult
	{
		public ResultadoLogin Resultado { get; set; }

		public string Mensaje { get; set; } = string.Empty;

		public string? Token { get; set; }

		public DateTime? Expira { get; set; }

		public bool DebeCambiarContrasena { get; set; }

		public DateTime? BloqueadaHasta { get; set; }
	}
}