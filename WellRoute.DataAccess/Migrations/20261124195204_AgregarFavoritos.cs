using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WellRoute.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFavoritos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Favoritos",
                columns: table => new
                {
                    UsuarioID = table.Column<int>(type: "int", nullable: false),
                    DestinoID = table.Column<int>(type: "int", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Favoritos", x => new { x.UsuarioID, x.DestinoID });
                    table.ForeignKey(
                        name: "FK_Favoritos_Destinos_DestinoID",
                        column: x => x.DestinoID,
                        principalTable: "Destinos",
                        principalColumn: "DestinoID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Favoritos_Usuarios_UsuarioID",
                        column: x => x.UsuarioID,
                        principalTable: "Usuarios",
                        principalColumn: "UsuarioID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Favoritos_DestinoID",
                table: "Favoritos",
                column: "DestinoID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Favoritos");
        }
    }
}
