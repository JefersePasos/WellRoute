using WellRoute.Arquitecture.Auditoria;
using WellRoute.Arquitecture.Auth;

namespace WellRoute.API
{
	public class ContextoSolicitud : IContextoSolicitud
	{
		private readonly IHttpContextAccessor _http;

		public ContextoSolicitud(IHttpContextAccessor http)
		{
			_http = http;
		}

		public int? UsuarioID => _http.HttpContext?.User.ObtenerUsuarioId();

		public string? DireccionIP => _http.HttpContext?.Connection.RemoteIpAddress?.ToString();

		public string? UserAgent => _http.HttpContext?.Request.Headers.UserAgent.ToString();
	}
}
