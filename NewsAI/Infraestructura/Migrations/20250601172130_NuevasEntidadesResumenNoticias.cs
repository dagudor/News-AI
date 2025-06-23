using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class NuevasEntidadesResumenNoticias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuracion_usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfiguracionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuracion_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Configuracion_usuarios_Configuraciones_ConfiguracionId",
                        column: x => x.ConfiguracionId,
                        principalTable: "Configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Configuracion_usuarios_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Noticia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Contenido = table.Column<string>(type: "TEXT", nullable: false),
                    Url = table.Column<string>(type: "TEXT", nullable: false),
                    FechaPublicacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Fuente = table.Column<string>(type: "TEXT", nullable: false),
                    Hashtags = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Noticia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Resumen",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ConfiguracionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UrlOrigen = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    ContenidoResumen = table.Column<string>(type: "TEXT", nullable: false),
                    NoticiasProcesadas = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaGeneracion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TiempoProcesamiento = table.Column<double>(type: "REAL", nullable: false),
                    EmailEnviado = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resumen_Configuraciones_ConfiguracionId",
                        column: x => x.ConfiguracionId,
                        principalTable: "Configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Resumen_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_usuarios_ConfiguracionId",
                table: "Configuracion_usuarios",
                column: "ConfiguracionId");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_usuarios_UsuarioId",
                table: "Configuracion_usuarios",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumen_ConfiguracionId",
                table: "Resumen",
                column: "ConfiguracionId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumen_UsuarioId",
                table: "Resumen",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Configuracion_usuarios");

            migrationBuilder.DropTable(
                name: "Noticia");

            migrationBuilder.DropTable(
                name: "Resumen");
        }
    }
}
