namespace RecoleccionResiduosApi.Models
{
    public class ConfiguracionZona
    {
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }
        
        public int TipoResiduoId { get; set; }
        public TipoResiduo? TipoResiduo { get; set; }
        
        public int FrecuenciaDias { get; set; } // Cada cuántos días se puede recolectar
        public double PesoMinimoKg { get; set; } = 0;
        public double PesoMaximoKg { get; set; } = 100;
        public TimeSpan HoraInicio { get; set; } = new TimeSpan(6, 0, 0); // 6:00 AM
        public TimeSpan HoraFin { get; set; } = new TimeSpan(18, 0, 0); // 6:00 PM
        public bool RequiereValidacionFoto { get; set; } = false;
        public bool Activa { get; set; } = true;
    }
}
