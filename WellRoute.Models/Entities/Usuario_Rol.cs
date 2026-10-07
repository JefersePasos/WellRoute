using System;

namespace WellRoute.Models.Entities
{
    public class Usuario_Rol
    {
        public int UsuarioID { get; set; }

        public int RolID { get; set; }

        public DateTime FechaAsignacion { get; set; }

        // Relaciones: Foreign Keys
        public Usuario Usuario { get; set; }

        public Rol Rol { get; set; }
    }
}