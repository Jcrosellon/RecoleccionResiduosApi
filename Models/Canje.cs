using System;

namespace RecoleccionResiduosApi.Models
{
    public class Canje
    {
        public int Id { get; set; }
        public DateTime FechaCanje { get; set; }
        public int PuntosUtilizados { get; set; }
        public string CodigoCanje { get; set; } = string.Empty; // Código único para usar el descuento
        public bool Utilizado { get; set; } = false;
        public DateTime? FechaUtilizacion { get; set; }

        // Relaciones
        public int UsuarioId { get; set; }
        public Usuario? Usuario { get; set; }

        public int DescuentoId { get; set; }
        public Descuento? Descuento { get; set; }
    }
}
