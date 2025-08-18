using System;

namespace RecoleccionResiduosApi.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string TipoNotificacion { get; set; } = string.Empty; // "WhatsApp", "Email", "Sistema"
        public DateTime FechaEnvio { get; set; }
        public bool Enviada { get; set; } = false;

        // Destinos según el tipo
        public string? NumeroWhatsApp { get; set; }
        public string? Email { get; set; }
        public string? Asunto { get; set; }

        // Trazabilidad
        public int Intentos { get; set; } = 0;
        public string? RespuestaApi { get; set; }
        public string? ErrorMensaje { get; set; }

        // Relaciones
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }
        public int? RecoleccionId { get; set; }
        public Recoleccion? Recoleccion { get; set; }
    }
}
