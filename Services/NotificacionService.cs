using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Services
{
    public class NotificacionService
    {
        private readonly AppDbContext _context;
        private readonly IWhatsAppService _whatsAppService;
        private readonly IEmailService _emailService;
        private readonly ILogger<NotificacionService> _logger;

        public NotificacionService(
            AppDbContext context,
            IWhatsAppService whatsAppService,
            IEmailService emailService,
            ILogger<NotificacionService> logger)
        {
            _context = context;
            _whatsAppService = whatsAppService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<bool> EnviarNotificacionAsync(
            int usuarioId,
            string mensaje,
            string tipo = "WhatsApp",
            int? recoleccionId = null,
            string? email = null,
            string? asunto = null,
            string? telefono = null)
        {
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario is null)
                {
                    _logger.LogWarning("Usuario {UsuarioId} no encontrado", usuarioId);
                    return false;
                }

                var notif = new Notificacion
                {
                    UsuarioId = usuarioId,
                    Mensaje = mensaje,
                    TipoNotificacion = tipo,
                    FechaEnvio = DateTime.UtcNow,
                    RecoleccionId = recoleccionId,
                    Intentos = 1
                };

                bool enviado = false;

                if (tipo == "WhatsApp")
                {
                    var destino = string.IsNullOrWhiteSpace(telefono) ? usuario.Telefono : telefono;
                    if (string.IsNullOrWhiteSpace(destino))
                    {
                        _logger.LogWarning("Usuario {UsuarioId} sin teléfono para WhatsApp", usuarioId);
                        notif.ErrorMensaje = "Sin teléfono";
                    }
                    else
                    {
                        notif.NumeroWhatsApp = destino;
                        enviado = await _whatsAppService.EnviarMensajeAsync(destino, mensaje);
                        notif.RespuestaApi = enviado ? "OK" : "ERROR";
                        if (!enviado) notif.ErrorMensaje = "Fallo envío WhatsApp";
                    }
                }
                else if (tipo == "Email")
                {
                    var destino = string.IsNullOrWhiteSpace(email) ? usuario.Email : email;
                    var subj = string.IsNullOrWhiteSpace(asunto) ? "Notificación EcoWaste" : asunto;

                    if (string.IsNullOrWhiteSpace(destino))
                    {
                        _logger.LogWarning("Usuario {UsuarioId} sin email para notificación", usuarioId);
                        notif.ErrorMensaje = "Sin email";
                    }
                    else
                    {
                        notif.Email = destino;
                        notif.Asunto = subj;

                        // Puedes envolver el mensaje si quieres HTML
                        var html = $@"
                            <div style=""font-family:Inter,Arial,sans-serif;max-width:560px;margin:auto;border:1px solid #e5e7eb;border-radius:12px;overflow:hidden"">
                              <div style=""background:#059669;color:#111827;padding:16px;display:flex;align-items:center;gap:12px"">
                                <div style=""width:36px;height:36px;border-radius:9999px;background:rgba(255,255,255,.15);display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff"">♻️</div>
                                <div style=""font-weight:600;font-size:18px"">EcoWaste</div>
                              </div>
                              <div style=""padding:20px;color:#111827;font-size:14px;line-height:1.6"">
                                <p>{mensaje}</p>
                              </div>
                              <div style=""background:#f9fafb;color:#6b7280;padding:12px 20px;font-size:12px"">
                                Este es un mensaje automático. Por favor, no respondas a este correo.
                              </div>
                            </div>";

                        try
                        {
                            await _emailService.SendAsync(destino, subj, html);
                            enviado = true;
                            notif.RespuestaApi = "OK";
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error enviando email a {Email}", destino);
                            notif.ErrorMensaje = ex.Message;
                        }
                    }
                }
                else
                {
                    // "Sistema" u otros tipos: solo registra como enviado internamente
                    enviado = true;
                    notif.RespuestaApi = "SISTEMA";
                }

                notif.Enviada = enviado;

                _context.Notificaciones.Add(notif);
                await _context.SaveChangesAsync();

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificación a usuario {UsuarioId}", usuarioId);
                return false;
            }
        }

        public async Task<bool> EnviarNotificacionTemplateAsync(int usuarioId, string templateName, Dictionary<string, string> parametros, int? recoleccionId = null)
        {
            // (igual que tenías, puedes mantenerlo para WhatsApp)
            try
            {
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null || string.IsNullOrEmpty(usuario.Telefono))
                    return false;

                parametros["nombre"] = usuario.Nombre;

                bool enviado = await _whatsAppService.EnviarMensajeTemplateAsync(usuario.Telefono, templateName, parametros);

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
                    Intentos = 1,
                    RespuestaApi = enviado ? "Template enviado" : "Error enviando template",
                    ErrorMensaje = enviado ? null : "Error enviando template"
                };

                _context.Notificaciones.Add(notificacion);
                await _context.SaveChangesAsync();

                return enviado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando template {Template} a usuario {UsuarioId}", templateName, usuarioId);
                return false;
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

        /// Notifica que una recolección fue confirmada (usa plantilla)
    public async Task NotificarRecoleccionConfirmadaAsync(int recoleccionId)
    {
        var recoleccion = await _context.Recolecciones
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == recoleccionId);

        if (recoleccion == null || recoleccion.Usuario == null)
        {
            _logger.LogWarning("No se encontró recolección {RecoleccionId} o sin usuario.", recoleccionId);
            return;
        }

        var parametros = new Dictionary<string, string>
        {
            ["peso"] = recoleccion.PesoKg?.ToString("F1") ?? "0",
            ["puntos"] = recoleccion.PuntosGanados.ToString()
        };

        await EnviarNotificacionTemplateAsync(
            recoleccion.UsuarioId,
            "recoleccion_confirmada",
            parametros,
            recoleccionId
        );
    }

    /// Notifica los puntos acumulados del usuario (usa plantilla)
    public async Task NotificarPuntosAcumuladosAsync(int usuarioId)
    {
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            _logger.LogWarning("No se encontró el usuario {UsuarioId} para notificar puntos.", usuarioId);
            return;
        }

        var parametros = new Dictionary<string, string>
        {
            ["puntos"] = usuario.Puntos.ToString()
        };

        await EnviarNotificacionTemplateAsync(
            usuarioId,
            "puntos_acumulados",
            parametros
        );
    }
    }
}
