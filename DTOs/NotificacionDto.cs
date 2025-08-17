namespace RecoleccionResiduosApi.DTOs
{
    public class NotificacionDto
    {
        public int Id { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string TipoNotificacion { get; set; } = string.Empty;
        public DateTime FechaEnvio { get; set; }
        public bool Enviada { get; set; }
        public string? NumeroWhatsApp { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public int? RecoleccionId { get; set; }
    }

    public class CrearNotificacionDto
    {
        public int UsuarioId { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string TipoNotificacion { get; set; } = "WhatsApp";
        public int? RecoleccionId { get; set; }
    }

    public class EnviarTemplateDto
    {
        public int UsuarioId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public Dictionary<string, string> Parametros { get; set; } = new();
        public int? RecoleccionId { get; set; }
    }
}
