using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Services
{
    public class NotificacionService
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(AppDbContext context, IWhatsAppService whatsAppService, ILogger<NotificacionService> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _logger = logger;
        }

        public async Task<bool> EnviarNotificacionAsync(int usuarioId, string mensaje, string tipo = "WhatsApp", int? recoleccionId = null)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null || string.IsNullOrEmpty(usuario.Telefono))
                {
                    _logger.LogWarning($"Usuario {usuarioId} no encontrado o sin teléfono");
                    return false;
                }

                // Crear registro de notificación
                var notificacion = new Notificacion
                {
                    UsuarioId = usuarioId,
                    Mensaje = mensaje,
                    TipoNotificacion = tipo,
                    FechaEnvio = DateTime.UtcNow,
                    NumeroWhatsApp = usuario.Telefono,
                    RecoleccionId = recoleccionId
                };

                _context.Notificaciones.Add(notificacion);

                // Enviar mensaje por WhatsApp
                bool enviado = false;
                if (tipo == "WhatsApp")
                {
                    enviado = await _whatsAppService.EnviarMensajeAsync(usuario.Telefono, mensaje);
                }

                // Actualizar estado de la notificación
                notificacion.Enviada = enviado;
                if (enviado)
                {
                    notificacion.RespuestaApi = "Enviado exitosamente";
                }

                await _context.SaveChangesAsync();
                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando notificación a usuario {usuarioId}");
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionTemplateAsync(int usuarioId, string templateName, Dictionary<string, string> parametros, int? recoleccionId = null)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null || string.IsNullOrEmpty(usuario.Telefono))
                {
                    return false;
                }

                // Agregar nombre del usuario a los parámetros
                parametros["nombre"] = usuario.Nombre;

                bool enviado = await _whatsAppService.EnviarMensajeTemplateAsync(usuario.Telefono, templateName, parametros);

                // Registrar notificación
                var mensaje = GenerarMensajeTemplate(templateName, parametros);
                var notificacion = new Notificacion
                {
                    UsuarioId = usuarioId,
                    Mensaje = mensaje,
                    TipoNotificacion = "WhatsApp",
                    FechaEnvio = DateTime.UtcNow,
                    Enviada = enviado,
                    NumeroWhatsApp = usuario.Telefono,
                    RecoleccionId = recoleccionId,
                    RespuestaApi = enviado ? "Template enviado" : "Error enviando template"
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando template {templateName} a usuario {usuarioId}");
                return false;
            }
        }

        public async Task NotificarSolicitudCreadaAsync(int recoleccionId)
        {
            var recoleccion = await _context.Recolecciones
                .Include(r => r.Usuario)
                .Include(r => r.TipoResiduo)
                .FirstOrDefaultAsync(r => r.Id == recoleccionId);

            if (recoleccion != null)
            {
                var parametros = new Dictionary<string, string>
                {
                    ["fecha"] = recoleccion.Fecha.ToString("dd/MM/yyyy"),
                    ["tipo"] = recoleccion.TipoResiduo?.Nombre ?? "residuos"
                };

                await EnviarNotificacionTemplateAsync(recoleccion.UsuarioId, "solicitud_creada", parametros, recoleccionId);
            }
        }

        public async Task NotificarRecoleccionConfirmadaAsync(int recoleccionId)
        {
            var recoleccion = await _context.Recolecciones
                .Include(r => r.Usuario)
                .FirstOrDefaultAsync(r => r.Id == recoleccionId);

            if (recoleccion != null)
            {
                var parametros = new Dictionary<string, string>
                {
                    ["peso"] = recoleccion.PesoKg?.ToString("F1") ?? "0",
                    ["puntos"] = recoleccion.PuntosGanados.ToString()
                };

                await EnviarNotificacionTemplateAsync(recoleccion.UsuarioId, "recoleccion_confirmada", parametros, recoleccionId);
            }
        }

        public async Task NotificarPuntosAcumuladosAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario != null)
            {
                var parametros = new Dictionary<string, string>
                {
                    ["puntos"] = usuario.Puntos.ToString()
                };

                await EnviarNotificacionTemplateAsync(usuarioId, "puntos_acumulados", parametros);
            }
        }

        public async Task<List<Notificacion>> GetNotificacionesPorUsuarioAsync(int usuarioId)
        {
            return await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId)
                .OrderByDescending(n => n.FechaEnvio)
                .ToListAsync();
        }

        private string GenerarMensajeTemplate(string templateName, Dictionary<string, string> parametros)
        {
            return templateName switch
            {
                "solicitud_creada" => $"Solicitud registrada para {parametros.GetValueOrDefault("fecha", "")}",
                "recoleccion_confirmada" => $"Recolección confirmada: {parametros.GetValueOrDefault("peso", "")}kg, {parametros.GetValueOrDefault("puntos", "")} puntos",
                "puntos_acumulados" => $"Puntos acumulados: {parametros.GetValueOrDefault("puntos", "")}",
                _ => "Notificación del sistema"
            };
        }
    }
}
