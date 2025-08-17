using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;
using RecoleccionResiduosApi.Services;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificacionService _notificacionService;

        public NotificacionesController(AppDbContext context, NotificacionService notificacionService)
        {
            _context = context;
            _notificacionService = notificacionService;
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<NotificacionDto>>> GetNotificacionesPorUsuario(int usuarioId)
        {
            var notificaciones = await _context.Notificaciones
                .Include(n => n.Usuario)
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Mensaje = n.Mensaje,
                    TipoNotificacion = n.TipoNotificacion,
                    FechaEnvio = n.FechaEnvio,
                    Enviada = n.Enviada,
                    NumeroWhatsApp = n.NumeroWhatsApp,
                    UsuarioId = n.UsuarioId,
                    NombreUsuario = n.Usuario!.Nombre,
                    RecoleccionId = n.RecoleccionId
                })
                .ToListAsync();

            return Ok(notificaciones);
        }

        [HttpPost("enviar")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult> EnviarNotificacion(CrearNotificacionDto dto)
        {
            var resultado = await _notificacionService.EnviarNotificacionAsync(
                dto.UsuarioId, 
                dto.Mensaje, 
                dto.TipoNotificacion, 
                dto.RecoleccionId
            );

            if (resultado)
            {
                return Ok(new { mensaje = "Notificación enviada exitosamente" });
            }
            else
            {
                return BadRequest(new { mensaje = "Error enviando notificación" });
            }
        }

        [HttpPost("enviar-template")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult> EnviarTemplate(EnviarTemplateDto dto)
        {
            var resultado = await _notificacionService.EnviarNotificacionTemplateAsync(
                dto.UsuarioId,
                dto.TemplateName,
                dto.Parametros,
                dto.RecoleccionId
            );

            if (resultado)
            {
                return Ok(new { mensaje = "Template enviado exitosamente" });
            }
            else
            {
                return BadRequest(new { mensaje = "Error enviando template" });
            }
        }

        [HttpGet("historial")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<NotificacionDto>>> GetHistorialNotificaciones(
            [FromQuery] int? usuarioId = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var query = _context.Notificaciones
                .Include(n => n.Usuario)
                .AsQueryable();

            if (usuarioId.HasValue)
                query = query.Where(n => n.UsuarioId == usuarioId.Value);

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(n => n.TipoNotificacion == tipo);

            if (fechaDesde.HasValue)
                query = query.Where(n => n.FechaEnvio >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(n => n.FechaEnvio <= fechaHasta.Value);

            var notificaciones = await query
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Mensaje = n.Mensaje,
                    TipoNotificacion = n.TipoNotificacion,
                    FechaEnvio = n.FechaEnvio,
                    Enviada = n.Enviada,
                    NumeroWhatsApp = n.NumeroWhatsApp,
                    UsuarioId = n.UsuarioId,
                    NombreUsuario = n.Usuario!.Nombre,
                    RecoleccionId = n.RecoleccionId
                })
                .ToListAsync();

            return Ok(notificaciones);
        }

        [HttpPost("test-whatsapp")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult> TestWhatsApp([FromBody] TestWhatsAppDto dto)
        {
            var resultado = await _notificacionService.EnviarNotificacionAsync(
                dto.UsuarioId,
                "Mensaje de prueba del sistema de recolección de residuos. ¡Todo funciona correctamente! 🚀"
            );

            return Ok(new { enviado = resultado });
        }
    }

    public class TestWhatsAppDto
    {
        public int UsuarioId { get; set; }
    }
}
