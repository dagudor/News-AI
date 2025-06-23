using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NewsAI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarSchedulingAutomatico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjecucionesProgramadas_Resumen_ResumenGeneradoId",
                table: "EjecucionesProgramadas");

            migrationBuilder.DropForeignKey(
                name: "FK_Resumen_Configuraciones_ConfiguracionId",
                table: "Resumen");

            migrationBuilder.DropForeignKey(
                name: "FK_Resumen_Usuarios_UsuarioId",
                table: "Resumen");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Resumen",
                table: "Resumen");

            migrationBuilder.RenameTable(
                name: "Resumen",
                newName: "ResumenGenerado");

            migrationBuilder.RenameIndex(
                name: "IX_Resumen_UsuarioId",
                table: "ResumenGenerado",
                newName: "IX_ResumenGenerado_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Resumen_ConfiguracionId",
                table: "ResumenGenerado",
                newName: "IX_ResumenGenerado_ConfiguracionId");

            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Configuraciones",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "diaria",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true,
                oldDefaultValue: "diaria");

            migrationBuilder.AddColumn<string>(
                name: "DiasPersonalizados",
                table: "Configuraciones",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraEnvio",
                table: "Configuraciones",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 8, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "SchedulingActivo",
                table: "Configuraciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ResumenGenerado",
                table: "ResumenGenerado",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Noticia_FechaPublicacion",
                table: "Noticia",
                column: "FechaPublicacion");

            migrationBuilder.CreateIndex(
                name: "IX_Noticia_Fuente",
                table: "Noticia",
                column: "Fuente");

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_SchedulingQuery",
                table: "Configuraciones",
                columns: new[] { "Activa", "SchedulingActivo", "ProximaEjecucion" });

            migrationBuilder.CreateIndex(
                name: "IX_Configuracion_UsuarioScheduling",
                table: "Configuraciones",
                columns: new[] { "UsuarioId", "Activa", "SchedulingActivo" });

            migrationBuilder.CreateIndex(
                name: "IX_Configuraciones_SchedulingActivo",
                table: "Configuraciones",
                column: "SchedulingActivo");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenGenerado_FechaGeneracion",
                table: "ResumenGenerado",
                column: "FechaGeneracion");

            migrationBuilder.CreateIndex(
                name: "IX_ResumenGenerado_UsuarioId_FechaGeneracion",
                table: "ResumenGenerado",
                columns: new[] { "UsuarioId", "FechaGeneracion" });

            migrationBuilder.AddForeignKey(
                name: "FK_EjecucionesProgramadas_ResumenGenerado_ResumenGeneradoId",
                table: "EjecucionesProgramadas",
                column: "ResumenGeneradoId",
                principalTable: "ResumenGenerado",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ResumenGenerado_Configuraciones_ConfiguracionId",
                table: "ResumenGenerado",
                column: "ConfiguracionId",
                principalTable: "Configuraciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ResumenGenerado_Usuarios_UsuarioId",
                table: "ResumenGenerado",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EjecucionesProgramadas_ResumenGenerado_ResumenGeneradoId",
                table: "EjecucionesProgramadas");

            migrationBuilder.DropForeignKey(
                name: "FK_ResumenGenerado_Configuraciones_ConfiguracionId",
                table: "ResumenGenerado");

            migrationBuilder.DropForeignKey(
                name: "FK_ResumenGenerado_Usuarios_UsuarioId",
                table: "ResumenGenerado");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Noticia_FechaPublicacion",
                table: "Noticia");

            migrationBuilder.DropIndex(
                name: "IX_Noticia_Fuente",
                table: "Noticia");

            migrationBuilder.DropIndex(
                name: "IX_Configuracion_SchedulingQuery",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuracion_UsuarioScheduling",
                table: "Configuraciones");

            migrationBuilder.DropIndex(
                name: "IX_Configuraciones_SchedulingActivo",
                table: "Configuraciones");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ResumenGenerado",
                table: "ResumenGenerado");

            migrationBuilder.DropIndex(
                name: "IX_ResumenGenerado_FechaGeneracion",
                table: "ResumenGenerado");

            migrationBuilder.DropIndex(
                name: "IX_ResumenGenerado_UsuarioId_FechaGeneracion",
                table: "ResumenGenerado");

            migrationBuilder.DropColumn(
                name: "DiasPersonalizados",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "HoraEnvio",
                table: "Configuraciones");

            migrationBuilder.DropColumn(
                name: "SchedulingActivo",
                table: "Configuraciones");

            migrationBuilder.RenameTable(
                name: "ResumenGenerado",
                newName: "Resumen");

            migrationBuilder.RenameIndex(
                name: "IX_ResumenGenerado_UsuarioId",
                table: "Resumen",
                newName: "IX_Resumen_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_ResumenGenerado_ConfiguracionId",
                table: "Resumen",
                newName: "IX_Resumen_ConfiguracionId");

            migrationBuilder.AlterColumn<string>(
                name: "Frecuencia",
                table: "Configuraciones",
                type: "TEXT",
                nullable: true,
                defaultValue: "diaria",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldDefaultValue: "diaria");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Resumen",
                table: "Resumen",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EjecucionesProgramadas_Resumen_ResumenGeneradoId",
                table: "EjecucionesProgramadas",
                column: "ResumenGeneradoId",
                principalTable: "Resumen",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Resumen_Configuraciones_ConfiguracionId",
                table: "Resumen",
                column: "ConfiguracionId",
                principalTable: "Configuraciones",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Resumen_Usuarios_UsuarioId",
                table: "Resumen",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
