namespace RecoleccionResiduosApi.Models
{
    public class TipoResiduo
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Ej: Orgánico
        public int Puntos { get; set; }

        // Navegación opcional
        public ICollection<Recoleccion>? Recolecciones { get; set; }
    }
}
