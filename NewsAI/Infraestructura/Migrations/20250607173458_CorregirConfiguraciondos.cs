using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class CorregirConfiguraciondos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_Frecuencia",
                table: "Configuraciones");

            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Configuraciones",
                type: "TEXT",
                nullable: true,
                defaultValue: "diaria",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldDefaultValue: "diaria");

            migrationBuilder.AlterColumn<bool>(
                name: "Email",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<bool>(
                name: "Audio",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Configuraciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "diaria",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValue: "diaria");

            migrationBuilder.AlterColumn<bool>(
                name: "Email",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Audio",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_Frecuencia",
                table: "Configuraciones",
                column: "Frecuencia");
        }
    }
}
