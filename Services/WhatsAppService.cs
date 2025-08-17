using System.Text;
using System.Text.Json;

namespace RecoleccionResiduosApi.Services
{
    public class WhatsAppService : IWhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration, ILogger<WhatsAppService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> EnviarMensajeAsync(string numeroTelefono, string mensaje)
        {
            try
            {
                // Configuración para WhatsApp Business API (ejemplo con Twilio)
                var accountSid = _configuration["WhatsApp:AccountSid"];
                var authToken = _configuration["WhatsApp:AuthToken"];
                var fromNumber = _configuration["WhatsApp:FromNumber"];

                if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                {
                    _logger.LogWarning("Configuración de WhatsApp no encontrada. Simulando envío.");
                    return await SimularEnvioAsync(numeroTelefono, mensaje);
                }

                // Formatear número de teléfono
                var numeroFormateado = FormatearNumeroTelefono(numeroTelefono);

                // Preparar datos para Twilio WhatsApp API
                var requestData = new
                {
                    From = $"whatsapp:{fromNumber}",
                    To = $"whatsapp:{numeroFormateado}",
                    Body = mensaje
                };

                var json = JsonSerializer.Serialize(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Configurar autenticación básica
                var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                // Enviar mensaje
                var response = await _httpClient.PostAsync($"https://api.twilio.com/2010-04-01/Accounts/{accountSid}/Messages.json", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Mensaje WhatsApp enviado exitosamente a {numeroFormateado}");
                    return true;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Error enviando mensaje WhatsApp: {response.StatusCode} - {errorContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Excepción enviando mensaje WhatsApp a {numeroTelefono}");
                return false;
            }
        }

        public async Task<bool> EnviarMensajeTemplateAsync(string numeroTelefono, string templateName, Dictionary<string, string> parametros)
        {
            // Implementación para templates de WhatsApp Business
            var mensaje = GenerarMensajeDesdeTemplate(templateName, parametros);
            return await EnviarMensajeAsync(numeroTelefono, mensaje);
        }

        private async Task<bool> SimularEnvioAsync(string numeroTelefono, string mensaje)
        {
            // Simulación para desarrollo/testing
            await Task.Delay(500); // Simular latencia de red
            _logger.LogInformation($"[SIMULACIÓN] WhatsApp enviado a {numeroTelefono}: {mensaje}");
            return true;
        }

        private string FormatearNumeroTelefono(string numero)
        {
            // Remover caracteres no numéricos
            var numeroLimpio = new string(numero.Where(char.IsDigit).ToArray());
            
            // Agregar código de país si no lo tiene (Colombia +57)
            if (!numeroLimpio.StartsWith("57") && numeroLimpio.Length == 10)
            {
                numeroLimpio = "57" + numeroLimpio;
            }
            
            return numeroLimpio;
        }

        private string GenerarMensajeDesdeTemplate(string templateName, Dictionary<string, string> parametros)
        {
            return templateName switch
            {
                "solicitud_creada" => $"¡Hola {parametros.GetValueOrDefault("nombre", "")}! Tu solicitud de recolección ha sido registrada. Fecha programada: {parametros.GetValueOrDefault("fecha", "")}. ¡Gracias por cuidar el medio ambiente! 🌱",
                
                "recoleccion_confirmada" => $"¡Excelente {parametros.GetValueOrDefault("nombre", "")}! Tu recolección ha sido confirmada. Peso: {parametros.GetValueOrDefault("peso", "")}kg. Puntos ganados: {parametros.GetValueOrDefault("puntos", "")}. ¡Sigue así! ♻️",
                
                "puntos_acumulados" => $"¡Felicitaciones {parametros.GetValueOrDefault("nombre", "")}! Has acumulado {parametros.GetValueOrDefault("puntos", "")} puntos. ¡Ya puedes canjear descuentos increíbles! 🎉",
                
                "recordatorio_recoleccion" => $"Recordatorio: Mañana tenemos programada tu recolección de residuos {parametros.GetValueOrDefault("tipo", "")}. ¡No olvides tener todo listo! 📅",
                
                _ => $"Notificación del sistema de recolección de residuos."
            };
        }
    }
}
