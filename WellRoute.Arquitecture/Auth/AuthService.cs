using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WellRoute.Arquitecture.Security;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Dtos;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Auth
{
	public interface IAuthService
	{
		Task<LoginResult> LoginAsync(LoginRequest request);
	}

	public class AuthService : IAuthService
	{
		// Se calcula una sola vez. Sirve para que el login tarde igual cuando el correo no existe
		private static readonly (string Hash, string Salt) HashRelleno = new PasswordHasher().Hashear("relleno");

		private readonly WellRouteDbContext _context;
		private readonly IPasswordHasher _hasher;
		private readonly ITokenService _tokenService;
		private readonly AuthOptions _opciones;

		public AuthService(
			WellRouteDbContext context,
			IPasswordHasher hasher,
			ITokenService tokenService,
			IOptions<AuthOptions> opciones)
		{
			_context = context;
			_hasher = hasher;
			_tokenService = tokenService;
			_opciones = opciones.Value;
		}

		public async Task<LoginResult> LoginAsync(LoginRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrEmpty(request.Password))
			{
				return CredencialesInvalidas();
			}

			var ahora = DateTime.UtcNow;
			var email = request.Email.Trim();

			var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

			if (usuario == null)
			{
				_hasher.Verificar(request.Password, HashRelleno.Hash, HashRelleno.Salt);
				return CredencialesInvalidas();
			}

			if (usuario.FechaBloqueoHasta.HasValue && usuario.FechaBloqueoHasta.Value > ahora)
			{
				return CuentaBloqueada(usuario.FechaBloqueoHasta.Value, ahora);
			}

			if (usuario.FechaBloqueoHasta.HasValue)
			{
				usuario.FechaBloqueoHasta = null;
				usuario.IntentosFallidos = 0;
			}

			if (!_hasher.Verificar(request.Password, usuario.PasswordHash, usuario.PasswordSalt))
			{
				return await RegistrarIntentoFallidoAsync(usuario, ahora);
			}

			if (!usuario.Activo)
			{
				return new LoginResult
				{
					Resultado = ResultadoLogin.CuentaInactiva,
					Mensaje = "La cuenta está desactivada. Contacta a un administrador."
				};
			}

			var rolesActivos = _context.UsuarioRoles
				.Where(ur => ur.UsuarioID == usuario.UsuarioID && ur.Rol.Activo);

			var roles = await rolesActivos
				.Select(ur => ur.Rol.Nombre)
				.ToListAsync();

			var permisos = await rolesActivos
				.SelectMany(ur => ur.Rol.RolPermisos.Select(rp => rp.Permiso.Codigo))
				.Distinct()
				.ToListAsync();

			var (token, expira) = _tokenService.Generar(usuario, roles, permisos);

			usuario.IntentosFallidos = 0;
			usuario.FechaBloqueoHasta = null;
			usuario.FechaUltimoLogin = ahora;
			await _context.SaveChangesAsync();

			return new LoginResult
			{
				Resultado = ResultadoLogin.Exitoso,
				Mensaje = "Inicio de sesión exitoso.",
				Token = token,
				Expira = expira,
				DebeCambiarContrasena = usuario.DebeCambiarContrasena
			};
		}

		private async Task<LoginResult> RegistrarIntentoFallidoAsync(Usuario usuario, DateTime ahora)
		{
			usuario.IntentosFallidos++;

			if (usuario.IntentosFallidos >= _opciones.MaximoIntentos)
			{
				usuario.FechaBloqueoHasta = ahora.AddMinutes(_opciones.MinutosBloqueo);
				await _context.SaveChangesAsync();
				return CuentaBloqueada(usuario.FechaBloqueoHasta.Value, ahora);
			}

			await _context.SaveChangesAsync();
			return CredencialesInvalidas();
		}

		private static LoginResult CredencialesInvalidas()
		{
			return new LoginResult
			{
				Resultado = ResultadoLogin.CredencialesInvalidas,
				Mensaje = "Correo o contraseña incorrectos."
			};
		}

		private static LoginResult CuentaBloqueada(DateTime hasta, DateTime ahora)
		{
			var minutos = (int)Math.Ceiling((hasta - ahora).TotalMinutes);

			return new LoginResult
			{
				Resultado = ResultadoLogin.CuentaBloqueada,
				Mensaje = $"La cuenta está bloqueada temporalmente. Intenta de nuevo en {minutos} minuto(s).",
				BloqueadaHasta = hasta
			};
		}
	}
}