using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class updatedUsersandConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Output",
                table: "Configuraciones");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaAlta",
                table: "Usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Audio",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Email",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "FechaAlta",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Audio",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Configuraciones");

            migrationBuilder.AddColumn<string>(
                name: "Output",
                table: "Configuraciones",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
