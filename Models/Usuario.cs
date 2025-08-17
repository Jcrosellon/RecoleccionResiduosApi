namespace RecoleccionResiduosApi.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario"; // Usuario, Recolector, Administrador
        public int Puntos { get; set; } = 0;

        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int? LocalidadId { get; set; }
        public Localidad? Localidad { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
        public bool Activo { get; set; } = true;

        // Navegación inversa (1:N)
        public ICollection<Recoleccion> Recolecciones { get; set; } = new List<Recoleccion>();
        public ICollection<Canje> Canjes { get; set; } = new List<Canje>();
        public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpires { get; set; }

    }
}
