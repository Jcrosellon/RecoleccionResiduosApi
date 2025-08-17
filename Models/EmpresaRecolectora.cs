namespace RecoleccionResiduosApi.Models
{
    public class EmpresaRecolectora
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Activa { get; set; } = true;

        // Relación con localidades donde opera
        public int LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }

        // Navegación
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
    }
}
