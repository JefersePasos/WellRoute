namespace WellRoute.Arquitecture.Auditoria
{
	// Arquitecture no conoce ASP.NET, así que los datos de la petición actual
	// se piden por esta interfaz. La implementación vive en WellRoute.API.
	public interface IContextoSolicitud
	{
		// Id del usuario autenticado, o null si la petición es anónima
		int? UsuarioID { get; }

		string? DireccionIP { get; }

		string? UserAgent { get; }
	}
}
