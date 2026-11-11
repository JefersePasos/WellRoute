namespace WellRoute.Models.Entities
{
	public class Bitacora
	{
		public int BitacoraID { get; set; }

		public int? UsuarioID { get; set; }

		// Módulo que genera el registro (ej. "Destinos", "Favoritos", "Usuarios")
		public string Modulo { get; set; } = string.Empty;

		// Qué se hizo (ej. "Crear", "Editar", "Desactivar")
		public string Accion { get; set; } = string.Empty;

		// Sobre qué entidad y cuál registro (opcionales)
		public string? Entidad { get; set; }

		public string? EntidadID { get; set; }

		public string? Descripcion { get; set; }

		public DateTime Fecha { get; set; }

		public string? DireccionIP { get; set; }

		public Usuario? Usuario { get; set; }
	}
}
