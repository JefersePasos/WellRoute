using System;
using System.Collections.Generic;

namespace WellRoute.Models.Entities
{
    public class Usuario
    {
        public int UsuarioID { get; set; }

        public string Nombre { get; set; }

        public string Email { get; set; }

        public string PasswordHash { get; set; }

        public string PasswordSalt { get; set; }

        public string Telefono { get; set; }

        public string FotoPerfil { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaActualizacion { get; set; }

        public DateTime? FechaUltimoLogin { get; set; }

        // Relación: Un Usuario puede tener muchos Usuario_Rol
        public ICollection<Usuario_Rol> UsuarioRoles { get; set; } = new List<Usuario_Rol>();
    }
}