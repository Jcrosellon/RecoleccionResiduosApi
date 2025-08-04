namespace RecoleccionResiduosApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        // Ahora representa la contraseña hasheada
        public string PasswordHash { get; set; } = string.Empty;

        // Puede ser: Usuario, Recolector, Administrador
        public string Rol { get; set; } = "Usuario";

        public int Puntos { get; set; } = 0;

        // Navegación inversa (1:N)
        public List<Recoleccion> Recolecciones { get; set; } = new();
    }
}
