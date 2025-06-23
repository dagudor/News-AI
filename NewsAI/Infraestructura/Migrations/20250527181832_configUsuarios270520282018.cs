using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class configUsuarios270520282018 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionUsuarios_ConfiguracionId",
                table: "ConfiguracionUsuarios",
                column: "ConfiguracionId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionUsuarios_UsuarioId",
                table: "ConfiguracionUsuarios",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionUsuarios_Configuraciones_ConfiguracionId",
                table: "ConfiguracionUsuarios",
                column: "ConfiguracionId",
                principalTable: "Configuraciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConfiguracionUsuarios_Usuarios_UsuarioId",
                table: "ConfiguracionUsuarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionUsuarios_Configuraciones_ConfiguracionId",
                table: "ConfiguracionUsuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_ConfiguracionUsuarios_Usuarios_UsuarioId",
                table: "ConfiguracionUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionUsuarios_ConfiguracionId",
                table: "ConfiguracionUsuarios");

            migrationBuilder.DropIndex(
                name: "IX_ConfiguracionUsuarios_UsuarioId",
                table: "ConfiguracionUsuarios");
        }
    }
}
