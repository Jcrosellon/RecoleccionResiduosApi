namespace RecoleccionResiduosApi.DTOs
{
    public class LocalidadDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
    }

    public class CrearLocalidadDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;
    }
}
