namespace RecoleccionResiduosApi.DTOs
{
    public class ConfiguracionZonaDto
    {
        public int Id { get; set; }
        public int LocalidadId { get; set; }
        public string LocalidadNombre { get; set; } = string.Empty;
        public int TipoResiduoId { get; set; }
        public string TipoResiduoNombre { get; set; } = string.Empty;
        public int FrecuenciaDias { get; set; }
        public double PesoMinimoKg { get; set; }
        public double PesoMaximoKg { get; set; }
        public string HoraInicio { get; set; } = "06:00"; // ← string
        public string HoraFin { get; set; } = "18:00";    // ← string
        public bool RequiereValidacionFoto { get; set; }
        public bool Activa { get; set; }
    }
}
