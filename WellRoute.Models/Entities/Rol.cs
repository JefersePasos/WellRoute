using System;
using System.Collections.Generic;

namespace WellRoute.Models.Entities
{
    public class Rol
    {
        public int RolID { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Relación: Un Rol puede tener muchos Usuario_Rol
        public ICollection<Usuario_Rol> UsuarioRoles { get; set; } = new List<Usuario_Rol>();

        // Relación: Un Rol puede tener muchos Rol_Permiso
        public ICollection<Rol_Permiso> RolPermisos { get; set; } = new List<Rol_Permiso>();
    }
}