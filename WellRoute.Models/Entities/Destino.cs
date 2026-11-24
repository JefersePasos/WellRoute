namespace WellRoute.Models.Entities
{
	public class Destino
	{
		public int DestinoID { get; set; }

		public string Nombre { get; set; } = string.Empty;

		public string Pais { get; set; } = string.Empty;

		public string Categoria { get; set; } = string.Empty;

		public string Descripcion { get; set; } = string.Empty;

		public string ImagenUrl { get; set; } = string.Empty;

		public string? VideoUrl { get; set; }

		// false = el destino no aparece en la portada, pero se conserva
		public bool Activo { get; set; }

		public DateTime FechaCreacion { get; set; }

		public DateTime? FechaActualizacion { get; set; }
	}
}
