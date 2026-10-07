using System;
using System.Collections.Generic;

namespace WellRoute.Models.Entities
{
    public class Permiso
    {
        public int PermisoID { get; set; }

        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public string Codigo { get; set; }

        public DateTime FechaCreacion { get; set; }

        // Relación: Un Permiso puede tener muchos Rol_Permiso
        public ICollection<Rol_Permiso> RolPermisos { get; set; } = new List<Rol_Permiso>();
    }
}