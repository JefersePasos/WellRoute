using System;
using System.Collections.Generic;

namespace WellRoute.Models.Entities
{
	public class Usuario
	{
		public int UsuarioID { get; set; }

		public string Nombre { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string PasswordHash { get; set; } = string.Empty;

		public string PasswordSalt { get; set; } = string.Empty;

		public string Telefono { get; set; } = string.Empty;

		public string FotoPerfil { get; set; } = string.Empty;

		public bool Activo { get; set; }

		public DateTime FechaCreacion { get; set; }

		public DateTime? FechaActualizacion { get; set; }

		public DateTime? FechaUltimoLogin { get; set; }

		public int IntentosFallidos { get; set; }

		// null significa que la cuenta no está bloqueada
		public DateTime? FechaBloqueoHasta { get; set; }

		public bool DebeCambiarContrasena { get; set; }

		// Relación: Un Usuario puede tener muchos Usuario_Rol
		public ICollection<Usuario_Rol> UsuarioRoles { get; set; } = new List<Usuario_Rol>();
	}
}