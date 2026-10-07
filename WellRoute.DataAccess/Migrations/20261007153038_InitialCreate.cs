using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WellRoute.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    PermisoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.PermisoID);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RolID);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PasswordSalt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FotoPerfil = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaUltimoLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.UsuarioID);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolID = table.Column<int>(type: "int", nullable: false),
                    PermisoID = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolID, x.PermisoID });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoID",
                        column: x => x.PermisoID,
                        principalTable: "Permisos",
                        principalColumn: "PermisoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolID",
                        column: x => x.RolID,
                        principalTable: "Roles",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRoles",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    RolID = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRoles", x => new { x.UsuarioID, x.RolID });
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Roles_RolID",
                        column: x => x.RolID,
                        principalTable: "Roles",
                        principalColumn: "RolID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRoles_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "PermisoID", "Codigo", "Descripcion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, "user.create", "Permiso para crear nuevos usuarios", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Crear Usuario" },
                    { 2, "user.read", "Permiso para ver información de usuarios", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ver Usuario" },
                    { 3, "user.update", "Permiso para editar usuarios", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Editar Usuario" },
                    { 4, "user.deactivate", "Permiso para desactivar usuarios", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Desactivar Usuario" },
                    { 5, "destino.create", "Permiso para crear destinos", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Crear Destino" },
                    { 6, "destino.read", "Permiso para ver destinos", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Ver Destino" },
                    { 7, "destino.update", "Permiso para editar destinos", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Editar Destino" },
                    { 8, "destino.deactivate", "Permiso para desactivar destinos", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Desactivar Destino" },
                    { 9, "admin.access", "Permiso para acceder al panel administrativo", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Acceso Admin" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RolID", "Activo", "Descripcion", "FechaCreacion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Rol con acceso completo al sistema", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Administrador" },
                    { 2, true, "Rol para editar contenido", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Editor" },
                    { 3, true, "Rol estándar para usuarios regulares", new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Usuario" }
                });

            migrationBuilder.InsertData(
                table: "RolPermisos",
                columns: new[] { "PermisoID", "RolID", "FechaAsignacion" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 8, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 9, 1, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 7, 2, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, 3, new DateTime(2026, 10, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Codigo",
                table: "Permisos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoID",
                table: "RolPermisos",
                column: "PermisoID");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRoles_RolID",
                table: "UsuarioRoles",
                column: "RolID");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RolPermisos");

            migrationBuilder.DropTable(
                name: "UsuarioRoles");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
