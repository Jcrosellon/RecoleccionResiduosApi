using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecoleccionResiduosApi.DTOs;
using RecoleccionResiduosApi.Services;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ReportesController : ControllerBase
    {
        private readonly ReporteService _reporteService;

        public ReportesController(ReporteService reporteService)
        {
            _reporteService = reporteService;
        }

        [HttpGet("dashboard")]
        [Authorize(Roles = "Administrador,Recolector,Usuario")]
        public async Task<ActionResult<DashboardDto>> GetDashboard()
        {
            try
            {
                var dashboard = await _reporteService.GetDashboardAsync();
                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando dashboard", error = ex.Message });
            }
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<ReporteUsuarioDto>> GetReporteUsuario(
            int usuarioId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                // Verificar que el usuario solo pueda ver su propio reporte (excepto administradores)
                var usuarioIdClaim = User.FindFirst("UsuarioId")?.Value;
                var rolClaim = User.FindFirst("Rol")?.Value;
                
                if (rolClaim != "Administrador" && usuarioIdClaim != usuarioId.ToString())
                {
                    return Forbid("Solo puedes ver tu propio reporte");
                }

                var reporte = await _reporteService.GetReporteUsuarioAsync(usuarioId, fechaDesde, fechaHasta);
                return Ok(reporte);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando reporte de usuario", error = ex.Message });
            }
        }

        [HttpGet("localidad/{localidadId}")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult<ReporteLocalidadDto>> GetReporteLocalidad(
            int localidadId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var reporte = await _reporteService.GetReporteLocalidadAsync(localidadId, fechaDesde, fechaHasta);
                return Ok(reporte);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando reporte de localidad", error = ex.Message });
            }
        }

        [HttpGet("empresa/{empresaId}")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult<ReporteEmpresaDto>> GetReporteEmpresa(
            int empresaId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var reporte = await _reporteService.GetReporteEmpresaAsync(empresaId, fechaDesde, fechaHasta);
                return Ok(reporte);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando reporte de empresa", error = ex.Message });
            }
        }

        [HttpGet("estadisticas-generales")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<EstadisticaGeneralDto>>> GetEstadisticasGenerales()
        {
            try
            {
                var estadisticas = await _reporteService.GetEstadisticasGeneralesAsync();
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error generando estadísticas generales", error = ex.Message });
            }
        }

        [HttpGet("exportar/usuario/{usuarioId}")]
        public async Task<ActionResult> ExportarReporteUsuario(
            int usuarioId,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] string formato = "json")
        {
            try
            {
                // Verificar permisos
                var usuarioIdClaim = User.FindFirst("UsuarioId")?.Value;
                var rolClaim = User.FindFirst("Rol")?.Value;
                
                if (rolClaim != "Administrador" && usuarioIdClaim != usuarioId.ToString())
                {
                    return Forbid("Solo puedes exportar tu propio reporte");
                }

                var reporte = await _reporteService.GetReporteUsuarioAsync(usuarioId, fechaDesde, fechaHasta);

                if (formato.ToLower() == "csv")
                {
                    var csv = GenerarCsvUsuario(reporte);
                    return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"reporte_usuario_{usuarioId}.csv");
                }

                return Ok(reporte);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = "Error exportando reporte", error = ex.Message });
            }
        }

        private string GenerarCsvUsuario(ReporteUsuarioDto reporte)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Reporte de Usuario");
            csv.AppendLine($"Nombre,{reporte.Usuario.Nombre}");
            csv.AppendLine($"Email,{reporte.Usuario.Email}");
            csv.AppendLine($"Puntos Actuales,{reporte.Usuario.Puntos}");
            csv.AppendLine($"Total Recolecciones,{reporte.TotalRecolecciones}");
            csv.AppendLine($"Peso Total (Kg),{reporte.PesoTotalKg:F2}");
            csv.AppendLine($"Puntos Totales Ganados,{reporte.PuntosTotales}");
            csv.AppendLine($"Total Canjes,{reporte.TotalCanjes}");
            csv.AppendLine($"Puntos Canjeados,{reporte.PuntosCanjeados}");
            csv.AppendLine();
            
            csv.AppendLine("Recolecciones por Tipo");
            csv.AppendLine("Tipo,Cantidad,Peso Total,Puntos Total");
            foreach (var tipo in reporte.RecoleccionesPorTipo)
            {
                csv.AppendLine($"{tipo.TipoResiduo},{tipo.Cantidad},{tipo.PesoTotal:F2},{tipo.PuntosTotal}");
            }

            return csv.ToString();
        }
    }
}
