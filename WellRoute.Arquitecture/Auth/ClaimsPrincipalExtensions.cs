using System.Security.Claims;

namespace WellRoute.Arquitecture.Auth
{
	public static class ClaimsPrincipalExtensions
	{
		public static int? ObtenerUsuarioId(this ClaimsPrincipal usuario)
		{
			var valor = usuario.FindFirst(ClaimNombres.UsuarioId)?.Value;
			return int.TryParse(valor, out var id) ? id : null;
		}

		public static string? ObtenerEmail(this ClaimsPrincipal usuario)
		{
			return usuario.FindFirst(ClaimNombres.Email)?.Value;
		}

		public static string? ObtenerNombre(this ClaimsPrincipal usuario)
		{
			return usuario.FindFirst(ClaimNombres.Nombre)?.Value;
		}

		public static IEnumerable<string> ObtenerRoles(this ClaimsPrincipal usuario)
		{
			return usuario.FindAll(ClaimNombres.Rol).Select(c => c.Value);
		}

		public static IEnumerable<string> ObtenerPermisos(this ClaimsPrincipal usuario)
		{
			return usuario.FindAll(ClaimNombres.Permiso).Select(c => c.Value);
		}

		public static bool TienePermiso(this ClaimsPrincipal usuario, string permiso)
		{
			return usuario.HasClaim(ClaimNombres.Permiso, permiso);
		}

		public static bool DebeCambiarContrasena(this ClaimsPrincipal usuario)
		{
			return usuario.HasClaim(ClaimNombres.DebeCambiarContrasena, "true");
		}
	}
}