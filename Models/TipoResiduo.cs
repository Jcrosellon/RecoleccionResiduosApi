namespace RecoleccionResiduosApi.Models
{
    public class TipoResiduo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Ej: Orgánico, Inorgánico, Peligroso
        public int Puntos { get; set; }
        
        public string? Descripcion { get; set; }
        public string? Color { get; set; } // Color para identificación visual
        public bool Activo { get; set; } = true;

        // Navegación
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
        public ICollection<SubtipoResiduo> Subtipos { get; set; } = new List<SubtipoResiduo>();
    }
}
