namespace RecoleccionResiduosApi.Models
{
    public class SubtipoResiduo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Ej: "FO", "FV", "Poda"
        public string Descripcion { get; set; } = string.Empty; // Ej: "Fracción Orgánica"
        public int PuntosAdicionales { get; set; } = 0; // Puntos extra por separación correcta
        
        // Relación con tipo de residuo
        public int TipoResiduoId { get; set; }
        public TipoResiduo? TipoResiduo { get; set; }

        // Navegación
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
    }
}
