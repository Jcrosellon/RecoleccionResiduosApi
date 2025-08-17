namespace RecoleccionResiduosApi.DTOs
{
    public class RegistroRequest
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    // ===== NUEVOS DTOs públicos (basados en Usuario, sin PasswordHash) =====
    public class UsuarioProfileDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = "Usuario";
        public int Puntos { get; set; }
        public bool Activo { get; set; } = true;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public int? LocalidadId { get; set; }
        public DateTime FechaRegistro { get; set; }
    }

    public class UpdateUsuarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string Rol { get; set; } = "Usuario";
        public bool Activo { get; set; } = true;
    }

    public class UpdatePerfilDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
    }

    public class UpdatePuntosDto
    {
        public int Puntos { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

}
