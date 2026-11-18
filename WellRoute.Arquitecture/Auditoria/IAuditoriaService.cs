namespace WellRoute.Arquitecture.Auditoria
{
	public interface IAuditoriaService
	{
		// Historial_Accesos. Si no se pasa usuarioID se toma el del usuario autenticado.
		Task RegistrarAccesoAsync(TipoAcceso tipo, int? usuarioID = null, string? emailIntentado = null);

		// Bitacora. El usuario, la IP y la fecha se llenan solos.
		Task RegistrarAsync(
			string modulo,
			string accion,
			string? entidad = null,
			string? entidadID = null,
			string? descripcion = null);
	}
}
