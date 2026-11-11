namespace WellRoute.Arquitecture.Auditoria
{
	// Usar estas constantes en lugar de strings sueltos para que la bitácora
	// quede consistente y se pueda filtrar por módulo/acción.
	public static class ModulosBitacora
	{
		public const string Autenticacion = "Autenticacion";
		public const string Usuarios = "Usuarios";
		public const string Destinos = "Destinos";
		public const string Favoritos = "Favoritos";
	}

	public static class AccionesBitacora
	{
		public const string Crear = "Crear";
		public const string Editar = "Editar";
		public const string Desactivar = "Desactivar";
		public const string Activar = "Activar";
		public const string Eliminar = "Eliminar";
		public const string Consultar = "Consultar";
		public const string CambiarContrasena = "CambiarContrasena";
	}
}
