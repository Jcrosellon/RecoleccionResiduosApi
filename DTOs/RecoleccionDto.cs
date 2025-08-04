namespace RecoleccionResiduosApi.DTOs
{
    public class RecoleccionDto
    {
        public int Id { get; set; }
        public string TipoResiduo { get; set; } = string.Empty;
        public string? Subtipo { get; set; }
        public DateTime Fecha { get; set; }
        public double? PesoKg { get; set; }
        public bool EsValida { get; set; }
        public int PuntosGanados { get; set; }
    }
}
