using System;

namespace RecoleccionResiduosApi.Models
{
    public class Notificacion
    {
        public int Id { get; set; }
        public string Mensaje { get; set; } = string.Empty;
        public string TipoNotificacion { get; set; } = string.Empty; // "WhatsApp", "Email", "SMS"
        public DateTime FechaEnvio { get; set; }
        public bool Enviada { get; set; } = false;
        public string? NumeroWhatsApp { get; set; }
        public string? RespuestaApi { get; set; } // Respuesta de la API de WhatsApp

        // Relación con usuario
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        // Relación opcional con recolección
        public int? RecoleccionId { get; set; }
        public Recoleccion? Recoleccion { get; set; }
    }
}
