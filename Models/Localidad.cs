namespace RecoleccionResiduosApi.Models
{
    public class Localidad
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Ej: "Chapinero", "Usaquén"
        public string Ciudad { get; set; } = string.Empty; // Ej: "Bogotá"
        public string Departamento { get; set; } = string.Empty; // Ej: "Cundinamarca"
        public bool Activa { get; set; } = true;

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
        public ICollection<EmpresaRecolectora> EmpresasRecolectoras { get; set; } = new List<EmpresaRecolectora>();
    }
}
