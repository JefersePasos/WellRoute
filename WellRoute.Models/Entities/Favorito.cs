namespace WellRoute.Models.Entities
{
	// Un usuario guarda un destino como favorito. La llave es (UsuarioID, DestinoID): no se repite.
	public class Favorito
	{
		public int UsuarioID { get; set; }

		public int DestinoID { get; set; }

		public DateTime FechaCreacion { get; set; }

		public Usuario Usuario { get; set; } = null!;

		public Destino Destino { get; set; } = null!;
	}
}
