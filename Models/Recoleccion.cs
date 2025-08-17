using System;

namespace RecoleccionResiduosApi.Models
{
    public class Recoleccion
    {
        public int Id { get; set; }
        public int TipoResiduoId { get; set; }
        public TipoResiduo? TipoResiduo { get; set; }
        
        public int? SubtipoResiduoId { get; set; }
        public SubtipoResiduo? SubtipoResiduo { get; set; }
        
        public string? Subtipo { get; set; }
        
        public DateTime Fecha { get; set; }
        public double? PesoKg { get; set; }
        public bool EsValida { get; set; } = false;
        public int PuntosGanados { get; set; } = 0;
        
        public string? Observaciones { get; set; }
        public DateTime? FechaRecoleccion { get; set; } // Fecha real de recolección
        public string Estado { get; set; } = "Pendiente"; // Pendiente, Recolectado, Cancelado

        // Relaciones existentes
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int? EmpresaRecolectoraId { get; set; }
        public EmpresaRecolectora? EmpresaRecolectora { get; set; }

        public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    }
}
