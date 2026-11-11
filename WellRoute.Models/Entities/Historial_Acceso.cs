namespace WellRoute.Models.Entities
{
	public class Historial_Acceso
	{
		public int HistorialAccesoID { get; set; }

		// null cuando el correo no existe (login fallido de alguien que no está registrado)
		public int? UsuarioID { get; set; }

		// Solo se llena en eventos fallidos, para poder rastrear intentos contra una cuenta
		public string? EmailIntentado { get; set; }

		public string Evento { get; set; } = string.Empty;

		public DateTime Fecha { get; set; }

		public string? DireccionIP { get; set; }

		public string? UserAgent { get; set; }

		public Usuario? Usuario { get; set; }
	}
}
