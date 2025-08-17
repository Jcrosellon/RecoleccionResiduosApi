namespace RecoleccionResiduosApi.Models
{
    public class ReglaValidacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoRegla { get; set; } = string.Empty; // "Separacion", "Peso", "Frecuencia"
        public string Condicion { get; set; } = string.Empty; // JSON con condiciones
        public bool Activa { get; set; } = true;
        public int PuntosBonus { get; set; } = 0; // Puntos extra por cumplir la regla
        public int PuntosPenalizacion { get; set; } = 0; // Puntos que se restan por no cumplir

        // Relaciones opcionales
        public int? TipoResiduoId { get; set; }
        public TipoResiduo? TipoResiduo { get; set; }
        
        public int? LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }
    }
}
