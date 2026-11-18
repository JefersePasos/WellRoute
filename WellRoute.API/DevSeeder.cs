using Microsoft.EntityFrameworkCore;
using WellRoute.Arquitecture.Security;
using WellRoute.DataAccess.Context;
using WellRoute.Models.Entities;

namespace WellRoute.API
{
	// SOLO DESARROLLO. Crea un administrador de prueba si la tabla Usuarios está vacía.
	// Borrar este archivo (y su llamada en Program.cs) cuando el CRUD de usuarios (#6) ya permita crear usuarios reales.
	public static class DevSeeder
	{
		public const string EmailAdmin = "admin@wellroute.local";
		public const string PasswordTemporal = "Temporal#2026";

		public static async Task SembrarAsync(IServiceProvider servicios)
		{
			using var scope = servicios.CreateScope();
			var context = scope.ServiceProvider.GetRequiredService<WellRouteDbContext>();
			var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

			if (await context.Usuarios.AnyAsync())
			{
				return;
			}

			var (hash, salt) = hasher.Hashear(PasswordTemporal);

			var admin = new Usuario
			{
				Nombre = "Administrador",
				Email = EmailAdmin,
				PasswordHash = hash,
				PasswordSalt = salt,
				Telefono = string.Empty,
				FotoPerfil = string.Empty,
				Activo = true,
				FechaCreacion = DateTime.UtcNow,
				DebeCambiarContrasena = true
			};

			// RolID 1 = Administrador (viene del seed del DbContext)
			admin.UsuarioRoles.Add(new Usuario_Rol { RolID = 1, FechaAsignacion = DateTime.UtcNow });

			context.Usuarios.Add(admin);
			await context.SaveChangesAsync();
		}
	}
}
