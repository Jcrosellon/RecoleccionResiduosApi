using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecoleccionResiduosApi.Migrations
{
    /// <inheritdoc />
    public partial class NotificacionesEmailCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Asunto",
                table: "Notificaciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Notificaciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMensaje",
                table: "Notificaciones",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Intentos",
                table: "Notificaciones",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Asunto",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "ErrorMensaje",
                table: "Notificaciones");

            migrationBuilder.DropColumn(
                name: "Intentos",
                table: "Notificaciones");
        }
    }
}
