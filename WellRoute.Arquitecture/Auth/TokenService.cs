using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WellRoute.Models.Entities;

namespace WellRoute.Arquitecture.Auth
{
	public interface ITokenService
	{
		(string Token, DateTime Expira) Generar(Usuario usuario, IEnumerable<string> roles, IEnumerable<string> permisos);
	}

	public class TokenService : ITokenService
	{
		private readonly JwtOptions _opciones;

		public TokenService(IOptions<JwtOptions> opciones)
		{
			_opciones = opciones.Value;
		}

		public (string Token, DateTime Expira) Generar(Usuario usuario, IEnumerable<string> roles, IEnumerable<string> permisos)
		{
			if (string.IsNullOrWhiteSpace(_opciones.Key) || _opciones.Key.Length < 32)
			{
				throw new InvalidOperationException("Falta la clave Jwt:Key o es demasiado corta (mínimo 32 caracteres).");
			}

			var expira = DateTime.UtcNow.AddMinutes(_opciones.MinutosExpiracion);

			var claims = new List<Claim>
			{
				new(ClaimNombres.UsuarioId, usuario.UsuarioID.ToString()),
				new(ClaimNombres.Email, usuario.Email),
				new(ClaimNombres.Nombre, usuario.Nombre),
				new(ClaimNombres.DebeCambiarContrasena, usuario.DebeCambiarContrasena ? "true" : "false")
			};

			claims.AddRange(roles.Select(r => new Claim(ClaimNombres.Rol, r)));
			claims.AddRange(permisos.Select(p => new Claim(ClaimNombres.Permiso, p)));

			var descriptor = new SecurityTokenDescriptor
			{
				Subject = new ClaimsIdentity(claims),
				Issuer = _opciones.Issuer,
				Audience = _opciones.Audience,
				Expires = expira,
				SigningCredentials = new SigningCredentials(
					new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
					SecurityAlgorithms.HmacSha256)
			};

			var token = new JsonWebTokenHandler().CreateToken(descriptor);

			return (token, expira);
		}
	}
}