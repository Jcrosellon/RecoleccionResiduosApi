namespace RecoleccionResiduosApi.DTOs
{
    public class EmpresaRecolectoraDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public int LocalidadId { get; set; }
        public string LocalidadNombre { get; set; } = string.Empty;
    }
}
