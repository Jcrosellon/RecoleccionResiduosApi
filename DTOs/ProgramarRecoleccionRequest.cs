// DTOs/ProgramarRecoleccionRequest.cs
namespace RecoleccionResiduosApi.DTOs
{
    public class ProgramarRecoleccionRequest
    {
        public int TipoResiduoId { get; set; }
        public string? Subtipo { get; set; }
        public double PesoEstimado { get; set; }
        public DateTime FechaSolicitud { get; set; }
        public string? Observaciones { get; set; }
    }
}
