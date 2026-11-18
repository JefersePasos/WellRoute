using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellRoute.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AgregarHistorialAccesosYBitacora : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bitacora",
                columns: table => new
                {
                    BitacoraID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    Modulo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Entidad = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EntidadID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DireccionIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bitacora", x => x.BitacoraID);
                    table.ForeignKey(
                        name: "FK_Bitacora_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Historial_Accesos",
                columns: table => new
                {
                    HistorialAccesoID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioID = table.Column<int>(type: "int", nullable: true),
                    EmailIntentado = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Evento = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    DireccionIP = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historial_Accesos", x => x.HistorialAccesoID);
                    table.ForeignKey(
                        name: "FK_Historial_Accesos_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_Fecha",
                table: "Bitacora",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_Modulo_Accion",
                table: "Bitacora",
                columns: new[] { "Modulo", "Accion" });

            migrationBuilder.CreateIndex(
                name: "IX_Bitacora_UsuarioID",
                table: "Bitacora",
                column: "UsuarioID");

            migrationBuilder.CreateIndex(
                name: "IX_Historial_Accesos_Fecha",
                table: "Historial_Accesos",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_Historial_Accesos_UsuarioID",
                table: "Historial_Accesos",
                column: "UsuarioID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bitacora");

            migrationBuilder.DropTable(
                name: "Historial_Accesos");
        }
    }
}
