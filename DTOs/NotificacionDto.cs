using System.Text.Json.Serialization;

namespace RecoleccionResiduosApi.DTOs
{
   public class NotificacionDto
{
    public int Id { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string TipoNotificacion { get; set; } = string.Empty;

    public DateTime FechaEnvio { get; set; }

    [JsonPropertyName("estado")]
    public string Estado { get; set; } = "Pendiente";

    [JsonPropertyName("intentos")]
    public int Intentos { get; set; }

    [JsonPropertyName("errorMensaje")]
    public string? ErrorMensaje { get; set; }

    [JsonPropertyName("telefono")]
    public string? NumeroWhatsApp { get; set; }

    public string? Email { get; set; }
    public string? Asunto { get; set; }

    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public int? RecoleccionId { get; set; }
}


   public class CrearNotificacionDto
{
    public int UsuarioId { get; set; }
    public string Mensaje { get; set; } = string.Empty;

    [JsonPropertyName("tipo")]
    public string TipoNotificacion { get; set; } = "WhatsApp";

    public int? RecoleccionId { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Asunto { get; set; }
}


    public class EnviarTemplateDto
    {
        public int UsuarioId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> Parametros { get; set; } = new();
        public int? RecoleccionId { get; set; }
    }

    // Para el test directo que espera tu FE: { telefono, mensaje }
    public class TestWhatsAppDirectDto
    {
        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
    }
}
