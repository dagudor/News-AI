using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSistemaAutomatizado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Frecuencia",
                table: "Configuraciones",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "diaria");

            migrationBuilder.AddColumn<DateTime>(
                name: "ProximaEjecucion",
                table: "Configuraciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UltimaEjecucion",
                table: "Configuraciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId1",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EjecucionesProgramadas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfiguracionId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaEjecucion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false, defaultValue: "Pendiente"),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NoticiasEncontradas = table.Column<int>(type: "INTEGER", nullable: true),
                    NoticiasProcessadas = table.Column<int>(type: "INTEGER", nullable: true),
                    EmailEnviado = table.Column<bool>(type: "INTEGER", nullable: true),
                    MensajeError = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ResumenGeneradoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EjecucionesProgramadas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EjecucionesProgramadas_Configuraciones_ConfiguracionId",
                        column: x => x.ConfiguracionId,
                        principalTable: "Configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EjecucionesProgramadas_Resumen_ResumenGeneradoId",
                        column: x => x.ResumenGeneradoId,
                        principalTable: "Resumen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UrlsConfiables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Url = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TipoFuente = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "RSS"),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    UltimaExtraccion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ExtraccionesExitosas = table.Column<int>(type: "INTEGER", nullable: false),
                    ExtraccionesFallidas = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrlsConfiables", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UrlsConfiables_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionUrls",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ConfiguracionId = table.Column<int>(type: "INTEGER", nullable: false),
                    UrlConfiableId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "GETUTCDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionUrls", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionUrls_Configuraciones_ConfiguracionId",
                        column: x => x.ConfiguracionId,
                        principalTable: "Configuraciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfiguracionUrls_UrlsConfiables_UrlConfiableId",
                        column: x => x.UrlConfiableId,
                        principalTable: "UrlsConfiables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_Activa",
                table: "Configuraciones",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_Frecuencia",
                table: "Configuraciones",
                column: "Frecuencia");

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_ProximaEjecucion",
                table: "Configuraciones",
                column: "ProximaEjecucion");

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_UsuarioId1",
                table: "Configuraciones",
                column: "UsuarioId1");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionUrls_ConfiguracionId_UrlConfiableId",
                table: "ConfiguracionUrls",
                columns: new[] { "ConfiguracionId", "UrlConfiableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionUrls_UrlConfiableId",
                table: "ConfiguracionUrls",
                column: "UrlConfiableId");

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionesProgramadas_ConfiguracionId_Estado",
                table: "EjecucionesProgramadas",
                columns: new[] { "ConfiguracionId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionesProgramadas_Estado",
                table: "EjecucionesProgramadas",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionesProgramadas_FechaEjecucion",
                table: "EjecucionesProgramadas",
                column: "FechaEjecucion");

            migrationBuilder.CreateIndex(
                name: "IX_EjecucionesProgramadas_ResumenGeneradoId",
                table: "EjecucionesProgramadas",
                column: "ResumenGeneradoId");

            migrationBuilder.CreateIndex(
                name: "IX_UrlsConfiables_Activa",
                table: "UrlsConfiables",
                column: "Activa");

            migrationBuilder.CreateIndex(
                name: "IX_UrlsConfiables_UsuarioId_Url",
                table: "UrlsConfiables",
                columns: new[] { "UsuarioId", "Url" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Configuraciones_Usuarios_UsuarioId1",
                table: "Configuraciones",
                column: "UsuarioId1",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Configuraciones_Usuarios_UsuarioId1",
                table: "Configuraciones");

            migrationBuilder.DropTable(
                name: "ConfiguracionUrls");

            migrationBuilder.DropTable(
                name: "EjecucionesProgramadas");

            migrationBuilder.DropTable(
                name: "UrlsConfiables");

            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_Activa",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_Frecuencia",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_ProximaEjecucion",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_UsuarioId1",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "Activa",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "Frecuencia",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "ProximaEjecucion",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "UltimaEjecucion",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "UsuarioId1",
                table: "Configuraciones");
        }
    }
}
