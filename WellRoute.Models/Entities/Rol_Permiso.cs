using System;

namespace WellRoute.Models.Entities
{
    public class Rol_Permiso
    {
        public int RolID { get; set; }

        public int PermisoID { get; set; }

        public DateTime FechaAsignacion { get; set; }

        // Relaciones: Foreign Keys
        public Rol Rol { get; set; }

        public Permiso Permiso { get; set; }
    }
}