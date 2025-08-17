namespace RecoleccionResiduosApi.DTOs
{
    public class TipoResiduoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int PuntosPorKg { get; set; }
        public bool Activo { get; set; }
    }

    public class SubtipoResiduoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int TipoResiduoId { get; set; }
        public string TipoResiduoNombre { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
