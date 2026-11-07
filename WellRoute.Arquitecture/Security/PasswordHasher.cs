using System.Security.Cryptography;

namespace WellRoute.Arquitecture.Security
{
	public interface IPasswordHasher
	{
		(string Hash, string Salt) Hashear(string password);

		bool Verificar(string password, string hash, string salt);
	}

	public class PasswordHasher : IPasswordHasher
	{
		private const int TamanoSalt = 16;
		private const int TamanoHash = 32;

		// Si se cambia este número, los usuarios ya creados no van a poder entrar
		private const int Iteraciones = 600000;

		public (string Hash, string Salt) Hashear(string password)
		{
			var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
			var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

			return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
		}

		public bool Verificar(string password, string hash, string salt)
		{
			try
			{
				var saltBytes = Convert.FromBase64String(salt);
				var hashEsperado = Convert.FromBase64String(hash);

				if (hashEsperado.Length == 0)
				{
					return false;
				}

				var hashActual = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iteraciones, HashAlgorithmName.SHA256, hashEsperado.Length);

				return CryptographicOperations.FixedTimeEquals(hashActual, hashEsperado);
			}
			catch (FormatException)
			{
				return false;
			}
		}
	}
}