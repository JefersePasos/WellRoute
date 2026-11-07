namespace WellRoute.Arquitecture.Auth
{
	public class AuthOptions
	{
		public const string Seccion = "Auth";

		public int MaximoIntentos { get; set; } = 3;

		public int MinutosBloqueo { get; set; } = 15;
	}
}