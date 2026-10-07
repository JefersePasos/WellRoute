using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
using WellRoute.Models.Entities;

namespace WellRoute.DataAccess.Context
{
    public class WellRouteDbContext : DbContext
    {
        public WellRouteDbContext(DbContextOptions<WellRouteDbContext> options)
            : base(options)
        {
        }

        // DbSets para cada entidad
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<Usuario_Rol> UsuarioRoles { get; set; }
        public DbSet<Rol_Permiso> RolPermisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            var fechaSeed = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
            // ==================== CONFIGURACIÓN USUARIO ====================
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(e => e.UsuarioID);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired();

                entity.Property(e => e.PasswordSalt)
                    .IsRequired();

                entity.Property(e => e.Telefono)
                    .HasMaxLength(20);

                entity.Property(e => e.FotoPerfil)
                    .HasMaxLength(500);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relación: Un Usuario → Muchos Usuario_Rol
                entity.HasMany(u => u.UsuarioRoles)
                    .WithOne(ur => ur.Usuario)
                    .HasForeignKey(ur => ur.UsuarioID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== CONFIGURACIÓN ROL ====================
            modelBuilder.Entity<Rol>(entity =>
            {
                entity.HasKey(e => e.RolID);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Nombre)
                    .IsUnique();

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(255);

                entity.Property(e => e.Activo)
                    .HasDefaultValue(true);

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relación: Un Rol → Muchos Usuario_Rol
                entity.HasMany(r => r.UsuarioRoles)
                    .WithOne(ur => ur.Rol)
                    .HasForeignKey(ur => ur.RolID)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación: Un Rol → Muchos Rol_Permiso
                entity.HasMany(r => r.RolPermisos)
                    .WithOne(rp => rp.Rol)
                    .HasForeignKey(rp => rp.RolID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== CONFIGURACIÓN PERMISO ====================
            modelBuilder.Entity<Permiso>(entity =>
            {
                entity.HasKey(e => e.PermisoID);

                entity.Property(e => e.Nombre)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Descripcion)
                    .HasMaxLength(255);

                entity.Property(e => e.Codigo)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasIndex(e => e.Codigo)
                    .IsUnique();

                entity.Property(e => e.FechaCreacion)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Relación: Un Permiso → Muchos Rol_Permiso
                entity.HasMany(p => p.RolPermisos)
                    .WithOne(rp => rp.Permiso)
                    .HasForeignKey(rp => rp.PermisoID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ==================== CONFIGURACIÓN USUARIO_ROL ====================
            modelBuilder.Entity<Usuario_Rol>(entity =>
            {
                entity.HasKey(e => new { e.UsuarioID, e.RolID });

                entity.Property(e => e.FechaAsignacion)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ==================== CONFIGURACIÓN ROL_PERMISO ====================
            modelBuilder.Entity<Rol_Permiso>(entity =>
            {
                entity.HasKey(e => new { e.RolID, e.PermisoID });

                entity.Property(e => e.FechaAsignacion)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // ==================== SEED DATA: PERMISOS ====================
            modelBuilder.Entity<Permiso>().HasData(
                new Permiso { PermisoID = 1, Nombre = "Crear Usuario", Codigo = "user.create", Descripcion = "Permiso para crear nuevos usuarios", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 2, Nombre = "Ver Usuario", Codigo = "user.read", Descripcion = "Permiso para ver información de usuarios", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 3, Nombre = "Editar Usuario", Codigo = "user.update", Descripcion = "Permiso para editar usuarios", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 4, Nombre = "Desactivar Usuario", Codigo = "user.deactivate", Descripcion = "Permiso para desactivar usuarios", FechaCreacion = fechaSeed },

                new Permiso { PermisoID = 5, Nombre = "Crear Destino", Codigo = "destino.create", Descripcion = "Permiso para crear destinos", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 6, Nombre = "Ver Destino", Codigo = "destino.read", Descripcion = "Permiso para ver destinos", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 7, Nombre = "Editar Destino", Codigo = "destino.update", Descripcion = "Permiso para editar destinos", FechaCreacion = fechaSeed },
                new Permiso { PermisoID = 8, Nombre = "Desactivar Destino", Codigo = "destino.deactivate", Descripcion = "Permiso para desactivar destinos", FechaCreacion = fechaSeed },

                new Permiso { PermisoID = 9, Nombre = "Acceso Admin", Codigo = "admin.access", Descripcion = "Permiso para acceder al panel administrativo", FechaCreacion = fechaSeed }
            );

            // ==================== SEED DATA: ROLES ====================
            modelBuilder.Entity<Rol>().HasData(
                new Rol { RolID = 1, Nombre = "Administrador", Descripcion = "Rol con acceso completo al sistema", Activo = true, FechaCreacion = fechaSeed },
                new Rol { RolID = 2, Nombre = "Editor", Descripcion = "Rol para editar contenido", Activo = true, FechaCreacion = fechaSeed },
                new Rol { RolID = 3, Nombre = "Usuario", Descripcion = "Rol estándar para usuarios regulares", Activo = true, FechaCreacion = fechaSeed }
            );

            // ==================== SEED DATA: ROL_PERMISO ====================
            // Admin: todos los permisos
            modelBuilder.Entity<Rol_Permiso>().HasData(
                new Rol_Permiso { RolID = 1, PermisoID = 1, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 2, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 3, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 4, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 5, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 6, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 7, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 8, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 1, PermisoID = 9, FechaAsignacion = fechaSeed },

                // Editor: permisos de lectura y edición
                new Rol_Permiso { RolID = 2, PermisoID = 2, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 2, PermisoID = 3, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 2, PermisoID = 5, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 2, PermisoID = 6, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 2, PermisoID = 7, FechaAsignacion = fechaSeed },

                // Usuario: solo permisos de lectura
                new Rol_Permiso { RolID = 3, PermisoID = 2, FechaAsignacion = fechaSeed },
                new Rol_Permiso { RolID = 3, PermisoID = 6, FechaAsignacion = fechaSeed }
            );
        }
    }
}