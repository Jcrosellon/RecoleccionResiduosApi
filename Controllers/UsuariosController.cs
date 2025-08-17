using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;
using RecoleccionResiduosApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RecoleccionResiduosApi.Services;
using System.Linq; // <-- asegúrate de tener LINQ
using Microsoft.Extensions.Logging;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _jwtKey;
        private readonly ILogger<UsuariosController>? _logger;

        public UsuariosController(AppDbContext context, IConfiguration config, ILogger<UsuariosController> logger)
        {
            _context = context;
            _jwtKey = config["Jwt:Key"]!;
            _logger = logger;
        }


        [HttpPost("registro")]
        public async Task<IActionResult> Registro([FromBody] RegistroRequest request)
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == request.Email))
                return BadRequest("El email ya está registrado.");

            var usuario = new Usuario
            {
                Nombre = request.Nombre,
                Email = request.Email,
                Rol = request.Rol,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Activo = false // <-- NUEVO: queda pendiente de aprobación
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { usuario.Id, usuario.Email, usuario.Rol, usuario.Activo });
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

            if (!user.Activo)
                return StatusCode(StatusCodes.Status403Forbidden, "Tu cuenta está pendiente de aprobación.");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Rol)
        }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { token = tokenHandler.WriteToken(token) });
        }


        [Authorize]
        [HttpGet("perfil")]
        public async Task<IActionResult> Perfil()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var user = await _context.Usuarios.FindAsync(userId);

            if (user == null) return NotFound();
            return Ok(MapToDto(user));
        }

        // ====== NUEVO: PUT /api/Usuarios/perfil ======
        [Authorize]
        [HttpPut("perfil")]
        public async Task<ActionResult<UsuarioProfileDto>> UpdatePerfil([FromBody] UpdatePerfilDto dto)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == userId);
            if (u is null) return NotFound();

            u.Nombre = dto.Nombre;
            u.Telefono = dto.Telefono;
            u.Direccion = dto.Direccion;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(u));
        }

        // ====== NUEVO: GET /api/Usuarios (lista con filtros y query params) ======
        // Devuelve ARRAY (como espera tu FE que luego "simula" pagination).
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioProfileDto>>> GetUsuarios(
            [FromQuery] string? nombre,
            [FromQuery] string? email,
            [FromQuery] string? rol,
            [FromQuery] int? pageNumber,
            [FromQuery] int? pageSize)
        {
            var q = _context.Usuarios.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(nombre))
                q = q.Where(u => EF.Functions.Like(u.Nombre, $"%{nombre}%"));
            if (!string.IsNullOrWhiteSpace(email))
                q = q.Where(u => EF.Functions.Like(u.Email, $"%{email}%"));
            if (!string.IsNullOrWhiteSpace(rol))
                q = q.Where(u => u.Rol == rol);

            // Si quieres paginar en servidor, descomenta:
            // if (pageNumber.GetValueOrDefault() > 0 && pageSize.GetValueOrDefault() > 0)
            // {
            //     q = q.OrderBy(u => u.Id)
            //          .Skip(((pageNumber!.Value - 1) * pageSize!.Value))
            //          .Take(pageSize!.Value);
            // }

            var items = await q
                .OrderBy(u => u.Id)
                .Select(u => MapToDto(u))
                .ToListAsync();

            return Ok(items);
        }

        // ====== NUEVO: GET /api/Usuarios/{id} ======
        [HttpGet("{id:int}")]
        public async Task<ActionResult<UsuarioProfileDto>> GetById(int id)
        {
            var u = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();
            return Ok(MapToDto(u));
        }

        // ====== NUEVO: PUT /api/Usuarios/{id} ======
        // [Authorize(Roles = "Administrador")]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UsuarioProfileDto>> Update(int id, [FromBody] UpdateUsuarioDto dto)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            u.Nombre = dto.Nombre;
            u.Email = dto.Email;
            u.Telefono = dto.Telefono;
            u.Direccion = dto.Direccion;
            u.Rol = dto.Rol;
            u.Activo = dto.Activo;

            await _context.SaveChangesAsync();
            return Ok(MapToDto(u));
        }

        // ====== NUEVO: DELETE /api/Usuarios/{id} ======
        // Soft delete: marcar Activo=false (coincide con tu FE “desactivar”)
        // [Authorize(Roles = "Administrador")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            //DELETE
            // _context.Usuarios.Remove(u);
            u.Activo = false;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ====== NUEVO: PATCH /api/Usuarios/{id}/puntos ======
        // [Authorize(Roles = "Administrador,Recolector")]
        [HttpPatch("{id:int}/puntos")]
        public async Task<ActionResult<UsuarioProfileDto>> UpdatePuntos(int id, [FromBody] UpdatePuntosDto body)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();

            u.Puntos = body.Puntos;
            await _context.SaveChangesAsync();
            return Ok(MapToDto(u));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id:int}/aprobar")]
        public async Task<ActionResult<UsuarioProfileDto>> Aprobar(int id)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();
            u.Activo = true;
            await _context.SaveChangesAsync();
            return Ok(MapToDto(u));
        }

        [Authorize(Roles = "Administrador")]
        [HttpPatch("{id:int}/rechazar")]
        public async Task<ActionResult<UsuarioProfileDto>> Rechazar(int id)
        {
            var u = await _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id);
            if (u is null) return NotFound();
            u.Activo = false;
            await _context.SaveChangesAsync();
            return Ok(MapToDto(u));
        }


        // ===== Helper: mapeo seguro del modelo al DTO =====
        private static UsuarioProfileDto MapToDto(Usuario u) => new UsuarioProfileDto
        {
            Id = u.Id,
            Nombre = u.Nombre,
            Email = u.Email,
            Rol = u.Rol,
            Puntos = u.Puntos,
            Activo = u.Activo,
            Telefono = u.Telefono,
            Direccion = u.Direccion,
            LocalidadId = u.LocalidadId,
            FechaRegistro = u.FechaRegistro
        };
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(
         [FromBody] ForgotPasswordRequest body,
         [FromServices] IEmailService email,
         [FromServices] IConfiguration cfg,
         [FromServices] IWebHostEnvironment env)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == body.Email);
            if (user == null)
                return Ok(new { message = "Si el email existe, enviaremos instrucciones." });

            user.PasswordResetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "").Replace("/", "").Replace("=", "");
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendBase = cfg["FrontendBaseUrl"] ?? "http://localhost:4200";
            var resetUrl = $"{frontendBase}/reset-password?token={user.PasswordResetToken}";

            var html = $@"<p>Hola {user.Nombre},</p>
                      <p>Recibimos una solicitud para restablecer tu contraseña.</p>
                      <p><a href=""{resetUrl}"">Haz clic aquí para restablecerla</a></p>
                      <p>Este enlace expira en 1 hora.</p>
                      <p>Si no fuiste tú, ignora este correo.</p>";

            var sent = true;
            try { await email.SendAsync(user.Email, "Restablecer contraseña", html); }
            catch (Exception ex)
            {
                sent = false;
                _logger.LogError(ex, "SMTP error enviando a {Email}", user.Email);
            }


            if (env.IsDevelopment()) // 👈 en dev te devuelve el flag
                return Ok(new { message = "Si el email existe, enviaremos instrucciones.", sent });

            return Ok(new { message = "Si el email existe, enviaremos instrucciones." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest body)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.PasswordResetToken == body.Token &&
                u.PasswordResetTokenExpires != null &&
                u.PasswordResetTokenExpires > DateTime.UtcNow);

            if (user == null)
                return BadRequest("Token inválido o expirado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(body.NewPassword);
            user.PasswordResetToken = null;
            user.PasswordResetTokenExpires = null;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Contraseña actualizada correctamente." });
        }


    }
}
