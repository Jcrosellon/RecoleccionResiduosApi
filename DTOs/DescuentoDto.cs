namespace RecoleccionResiduosApi.DTOs
{
    public class DescuentoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int PuntosRequeridos { get; set; }
        public decimal ValorDescuento { get; set; }
        public bool EsPorcentaje { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadCanjeada { get; set; }
        public bool PuedeCanjearse { get; set; }
    }

    public class CrearDescuentoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int PuntosRequeridos { get; set; }
        public decimal ValorDescuento { get; set; }
        public bool EsPorcentaje { get; set; } = true;
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int CantidadDisponible { get; set; } = -1;
    }

    public class ActualizarDescuentoDto
    {
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? PuntosRequeridos { get; set; }
        public decimal? ValorDescuento { get; set; }
        public bool? EsPorcentaje { get; set; }
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public bool? Activo { get; set; }
        public int? CantidadDisponible { get; set; }
    }
}
