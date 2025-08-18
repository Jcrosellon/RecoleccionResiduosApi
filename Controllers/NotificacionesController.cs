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

        // ✅ Notificaciones de un usuario
        [HttpGet("usuario/{usuarioId:int}")]
        public async Task<ActionResult<List<NotificacionDto>>> GetNotificacionesPorUsuario(int usuarioId)
        {
            var list = await _context.Notificaciones
                .Include(n => n.Usuario)
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Mensaje = n.Mensaje,
                    TipoNotificacion = n.TipoNotificacion,
                    FechaEnvio = n.FechaEnvio,

                    Estado = n.Enviada ? "Enviado" : (n.ErrorMensaje != null ? "Error" : "Pendiente"),
                    Intentos = n.Intentos,
                    ErrorMensaje = n.ErrorMensaje,

                    NumeroWhatsApp = n.NumeroWhatsApp,
                    Email = n.Email,
                    Asunto = n.Asunto,

                    UsuarioId = n.UsuarioId,
                    NombreUsuario = n.Usuario!.Nombre,
                    RecoleccionId = n.RecoleccionId
                })
                .ToListAsync();

            return Ok(list);
        }

        // ✅ Historial (admin) — también usa el mismo mapeo que el FE
        [HttpGet("historial")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<NotificacionDto>>> GetHistorialNotificaciones(
            [FromQuery] int? usuarioId = null,
            [FromQuery] string? tipo = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var q = _context.Notificaciones
                .Include(n => n.Usuario)
                .AsQueryable();

            if (usuarioId.HasValue) q = q.Where(n => n.UsuarioId == usuarioId.Value);
            if (!string.IsNullOrEmpty(tipo)) q = q.Where(n => n.TipoNotificacion == tipo);
            if (fechaDesde.HasValue) q = q.Where(n => n.FechaEnvio >= fechaDesde.Value);
            if (fechaHasta.HasValue) q = q.Where(n => n.FechaEnvio <= fechaHasta.Value);

            var list = await q
                .OrderByDescending(n => n.FechaEnvio)
                .Select(n => new NotificacionDto
                {
                    Id = n.Id,
                    Mensaje = n.Mensaje,
                    TipoNotificacion = n.TipoNotificacion,
                    FechaEnvio = n.FechaEnvio,

                    Estado = n.Enviada ? "Enviado" : (n.ErrorMensaje != null ? "Error" : "Pendiente"),
                    Intentos = n.Intentos,
                    ErrorMensaje = n.ErrorMensaje,

                    NumeroWhatsApp = n.NumeroWhatsApp,
                    Email = n.Email,
                    Asunto = n.Asunto,

                    UsuarioId = n.UsuarioId,
                    NombreUsuario = n.Usuario!.Nombre,
                    RecoleccionId = n.RecoleccionId
                })
                .ToListAsync();

            return Ok(list);
        }

        // ✅ Envío libre
        [HttpPost("enviar")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult> EnviarNotificacion([FromBody] CrearNotificacionDto dto)
        {
            var ok = await _notificacionService.EnviarNotificacionAsync(
                usuarioId: dto.UsuarioId,
                mensaje: dto.Mensaje,
                tipo: dto.TipoNotificacion?.Trim(),
                recoleccionId: dto.RecoleccionId,
                email: dto.Email,
                asunto: dto.Asunto,
                telefono: dto.Telefono
            );

            return ok ? Ok(new { mensaje = "Notificación enviada exitosamente" })
                      : BadRequest(new { mensaje = "Error enviando notificación" });
        }

        // ✅ Envío por template
        [HttpPost("enviar-template")]
        [Authorize(Roles = "Administrador,Recolector")]
        public async Task<ActionResult> EnviarTemplate([FromBody] EnviarTemplateDto dto)
        {
            var ok = await _notificacionService.EnviarNotificacionTemplateAsync(
                dto.UsuarioId, dto.TemplateName, dto.Parametros, dto.RecoleccionId
            );

            return ok ? Ok(new { mensaje = "Template enviado exitosamente" })
                      : BadRequest(new { mensaje = "Error enviando template" });
        }

        // ✅ Test directo de WhatsApp (tel + mensaje)
        [HttpPost("test-whatsapp")]
        [Authorize(Roles = "Administrador")]
public async Task<ActionResult> TestWhatsApp([FromBody] TestWhatsAppDirectDto dto)
        {
            var anyUserId = await _context.Usuarios.Select(u => u.Id).FirstOrDefaultAsync();
            if (anyUserId == 0)
                return BadRequest(new { enviado = false, detalle = "No hay usuarios para registrar la prueba" });

            var ok = await _notificacionService.EnviarNotificacionAsync(
                usuarioId: anyUserId,
                mensaje: dto.Mensaje,
                tipo: "WhatsApp",
                telefono: dto.Telefono
            );
            return Ok(new { enviado = ok });
        }
    }


}
