using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RecoleccionResiduosApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Descuentos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    PuntosRequeridos = table.Column<int>(type: "INTEGER", nullable: false),
                    ValorDescuento = table.Column<decimal>(type: "TEXT", precision: 10, scale: 2, nullable: false),
                    EsPorcentaje = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CantidadDisponible = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Descuentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Localidades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Ciudad = table.Column<string>(type: "TEXT", nullable: false),
                    Departamento = table.Column<string>(type: "TEXT", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Localidades", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposResiduo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Puntos = table.Column<int>(type: "INTEGER", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Color = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposResiduo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmpresasRecolectoras",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    LocalidadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresasRecolectoras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresasRecolectoras_Localidades_LocalidadId",
                        column: x => x.LocalidadId,
                        principalTable: "Localidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: false),
                    PasswordHash = table.Column<string>(type: "TEXT", nullable: false),
                    Rol = table.Column<string>(type: "TEXT", nullable: false),
                    Puntos = table.Column<int>(type: "INTEGER", nullable: false),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    LocalidadId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Localidades_LocalidadId",
                        column: x => x.LocalidadId,
                        principalTable: "Localidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionesZona",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LocalidadId = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoResiduoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FrecuenciaDias = table.Column<int>(type: "INTEGER", nullable: false),
                    PesoMinimoKg = table.Column<double>(type: "REAL", nullable: false),
                    PesoMaximoKg = table.Column<double>(type: "REAL", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    RequiereValidacionFoto = table.Column<bool>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionesZona", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesZona_Localidades_LocalidadId",
                        column: x => x.LocalidadId,
                        principalTable: "Localidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConfiguracionesZona_TiposResiduo_TipoResiduoId",
                        column: x => x.TipoResiduoId,
                        principalTable: "TiposResiduo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReglasValidacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    TipoRegla = table.Column<string>(type: "TEXT", nullable: false),
                    Condicion = table.Column<string>(type: "TEXT", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    PuntosBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    PuntosPenalizacion = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoResiduoId = table.Column<int>(type: "INTEGER", nullable: true),
                    LocalidadId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasValidacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReglasValidacion_Localidades_LocalidadId",
                        column: x => x.LocalidadId,
                        principalTable: "Localidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ReglasValidacion_TiposResiduo_TipoResiduoId",
                        column: x => x.TipoResiduoId,
                        principalTable: "TiposResiduo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "SubtiposResiduo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    PuntosAdicionales = table.Column<int>(type: "INTEGER", nullable: false),
                    TipoResiduoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubtiposResiduo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubtiposResiduo_TiposResiduo_TipoResiduoId",
                        column: x => x.TipoResiduoId,
                        principalTable: "TiposResiduo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Canjes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FechaCanje = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PuntosUtilizados = table.Column<int>(type: "INTEGER", nullable: false),
                    CodigoCanje = table.Column<string>(type: "TEXT", nullable: false),
                    Utilizado = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaUtilizacion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    DescuentoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Canjes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Canjes_Descuentos_DescuentoId",
                        column: x => x.DescuentoId,
                        principalTable: "Descuentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Canjes_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Recolecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoResiduoId = table.Column<int>(type: "INTEGER", nullable: false),
                    SubtipoResiduoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Subtipo = table.Column<string>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PesoKg = table.Column<double>(type: "REAL", nullable: true),
                    EsValida = table.Column<bool>(type: "INTEGER", nullable: false),
                    PuntosGanados = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    FechaRecoleccion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpresaRecolectoraId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Recolecciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Recolecciones_EmpresasRecolectoras_EmpresaRecolectoraId",
                        column: x => x.EmpresaRecolectoraId,
                        principalTable: "EmpresasRecolectoras",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recolecciones_SubtiposResiduo_SubtipoResiduoId",
                        column: x => x.SubtipoResiduoId,
                        principalTable: "SubtiposResiduo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Recolecciones_TiposResiduo_TipoResiduoId",
                        column: x => x.TipoResiduoId,
                        principalTable: "TiposResiduo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Recolecciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Mensaje = table.Column<string>(type: "TEXT", nullable: false),
                    TipoNotificacion = table.Column<string>(type: "TEXT", nullable: false),
                    FechaEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Enviada = table.Column<bool>(type: "INTEGER", nullable: false),
                    NumeroWhatsApp = table.Column<string>(type: "TEXT", nullable: true),
                    RespuestaApi = table.Column<string>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecoleccionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Recolecciones_RecoleccionId",
                        column: x => x.RecoleccionId,
                        principalTable: "Recolecciones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notificaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Canjes_CodigoCanje",
                table: "Canjes",
                column: "CodigoCanje",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Canjes_DescuentoId",
                table: "Canjes",
                column: "DescuentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Canjes_UsuarioId",
                table: "Canjes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesZona_LocalidadId_TipoResiduoId",
                table: "ConfiguracionesZona",
                columns: new[] { "LocalidadId", "TipoResiduoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionesZona_TipoResiduoId",
                table: "ConfiguracionesZona",
                column: "TipoResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresasRecolectoras_LocalidadId",
                table: "EmpresasRecolectoras",
                column: "LocalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_RecoleccionId",
                table: "Notificaciones",
                column: "RecoleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificaciones_UsuarioId",
                table: "Notificaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_EmpresaRecolectoraId",
                table: "Recolecciones",
                column: "EmpresaRecolectoraId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_SubtipoResiduoId",
                table: "Recolecciones",
                column: "SubtipoResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_TipoResiduoId",
                table: "Recolecciones",
                column: "TipoResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_Recolecciones_UsuarioId",
                table: "Recolecciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasValidacion_LocalidadId",
                table: "ReglasValidacion",
                column: "LocalidadId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasValidacion_TipoResiduoId",
                table: "ReglasValidacion",
                column: "TipoResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_SubtiposResiduo_TipoResiduoId",
                table: "SubtiposResiduo",
                column: "TipoResiduoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_LocalidadId",
                table: "Usuarios",
                column: "LocalidadId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Canjes");

            migrationBuilder.DropTable(
                name: "ConfiguracionesZona");

            migrationBuilder.DropTable(
                name: "Notificaciones");

            migrationBuilder.DropTable(
                name: "ReglasValidacion");

            migrationBuilder.DropTable(
                name: "Descuentos");

            migrationBuilder.DropTable(
                name: "Recolecciones");

            migrationBuilder.DropTable(
                name: "EmpresasRecolectoras");

            migrationBuilder.DropTable(
                name: "SubtiposResiduo");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "TiposResiduo");

            migrationBuilder.DropTable(
                name: "Localidades");
        }
    }
}
