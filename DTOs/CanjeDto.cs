namespace RecoleccionResiduosApi.DTOs
{
    public class CanjeDto
    {
        public int Id { get; set; }
        public DateTime FechaCanje { get; set; }
        public int PuntosUtilizados { get; set; }
        public string CodigoCanje { get; set; } = string.Empty;
        public bool Utilizado { get; set; }
        public DateTime? FechaUtilizacion { get; set; }
        public int UsuarioId { get; set; }
        public string NombreUsuario { get; set; } = string.Empty;
        public DescuentoDto Descuento { get; set; } = new();
    }

    public class RealizarCanjeDto
    {
        public int DescuentoId { get; set; }
    }

    public class UtilizarCanjeDto
    {
        public string CodigoCanje { get; set; } = string.Empty;
    }

    public class ValidarCanjeDto
    {
        public string CodigoCanje { get; set; } = string.Empty;
    }
}
