using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WellRoute.Arquitecture.Auth;

namespace WellRoute.API.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public class RequierePermisoAttribute : Attribute, IAuthorizationFilter
	{
		private readonly string _permiso;

		public RequierePermisoAttribute(string permiso)
		{
			_permiso = permiso;
		}

		public void OnAuthorization(AuthorizationFilterContext context)
		{
			var usuario = context.HttpContext.User;

			if (usuario.Identity?.IsAuthenticated != true)
			{
				context.Result = new UnauthorizedResult();
				return;
			}

			if (!usuario.TienePermiso(_permiso))
			{
				context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
			}
		}
	}
}