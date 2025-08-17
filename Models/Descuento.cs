namespace RecoleccionResiduosApi.Models
{
    public class Descuento
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Ej: "10% descuento en supermercado"
        public string Descripcion { get; set; } = string.Empty;
        public int PuntosRequeridos { get; set; }
        public decimal ValorDescuento { get; set; } // Valor en pesos o porcentaje
        public bool EsPorcentaje { get; set; } = true;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; } = true;
        public int CantidadDisponible { get; set; } = -1; // -1 = ilimitado

        // Navegación
        public ICollection<Canje> Canjes { get; set; } = new List<Canje>();
    }
}
