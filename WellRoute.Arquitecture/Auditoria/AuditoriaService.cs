using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Auditoria
{
	public class AuditoriaService : IAuditoriaService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly IContextoSolicitud _contexto;
		private readonly ILogger<AuditoriaService> _logger;

		public AuditoriaService(
			IServiceScopeFactory scopeFactory,
			IContextoSolicitud contexto,
			ILogger<AuditoriaService> logger)
		{
			_scopeFactory = scopeFactory;
			_contexto = contexto;
			_logger = logger;
		}

		public async Task RegistrarAccesoAsync(TipoAcceso tipo, int? usuarioID = null, string? emailIntentado = null)
		{
			try
			{
				// DbContext propio: así el registro no se mezcla con los cambios pendientes
				// del que llama, ni se pierde si el que llama hace rollback.
				using var scope = _scopeFactory.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<WellRouteDbContext>();

				db.HistorialAccesos.Add(new Historial_Acceso
				{
					UsuarioID = usuarioID ?? _contexto.UsuarioID,
					EmailIntentado = Recortar(emailIntentado, 100),
					Evento = tipo.ToString(),
					Fecha = DateTime.UtcNow,
					DireccionIP = _contexto.DireccionIP,
					UserAgent = Recortar(_contexto.UserAgent, 300)
				});

				await db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				// Un fallo de auditoría no debe tumbar el login ni la operación del usuario
				_logger.LogError(ex, "No se pudo registrar el acceso {Tipo}", tipo);
			}
		}

		public async Task RegistrarAsync(
			string modulo,
			string accion,
			string? entidad = null,
			string? entidadID = null,
			string? descripcion = null)
		{
			try
			{
				using var scope = _scopeFactory.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<WellRouteDbContext>();

				db.Bitacoras.Add(new Bitacora
				{
					UsuarioID = _contexto.UsuarioID,
					Modulo = modulo,
					Accion = accion,
					Entidad = entidad,
					EntidadID = entidadID,
					Descripcion = Recortar(descripcion, 500),
					Fecha = DateTime.UtcNow,
					DireccionIP = _contexto.DireccionIP
				});

				await db.SaveChangesAsync();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "No se pudo registrar en bitácora {Modulo}/{Accion}", modulo, accion);
			}
		}

		private static string? Recortar(string? texto, int max)
			=> texto is { Length: > 0 } && texto.Length > max ? texto[..max] : texto;
	}
}
