using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class addTablasconfiguracionUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ActivaSNS",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ConfiguracionPorDefecto",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActivaSNS",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "ConfiguracionPorDefecto",
                table: "Configuraciones");
        }
    }
}
