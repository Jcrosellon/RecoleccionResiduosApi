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
        private readonly ILogger<UsuariosController> _logger;

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
            // Seguridad: no revelar si existe o no
            if (user == null)
                return Ok(new { message = "Si el email existe, enviaremos instrucciones." });

            // Generar token y expiración
            user.PasswordResetToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
                .Replace("+", "").Replace("/", "").Replace("=", "");
            user.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(1);
            await _context.SaveChangesAsync();

            var frontendBase = cfg["FrontendBaseUrl"] ?? "http://localhost:4200";
            var resetUrl = $"{frontendBase}/reset-password?token={user.PasswordResetToken}";
            var siteUrl = frontendBase;

            var subject = "[EcoWaste] Restablecer tu contraseña";

            var html = $@"<!doctype html>
<html lang=""es"">
<head>
  <meta charset=""utf-8"">
  <meta name=""viewport"" content=""width=device-width,initial-scale=1"">
  <title>EcoWaste – Restablecer contraseña</title>
  <style>
    @media (prefers-color-scheme: dark) {{
      .card {{ background:#0b1410 !important; }}
      .text {{ color:#e5e7eb !important; }}
      .muted {{ color:#9ca3af !important; }}
      .footer {{ color:#9ca3af !important; border-top-color:#1f2937 !important; }}
    }}
  </style>
</head>
<body style=""margin:0;padding:0;background:#f0f9f4;"">
  <!-- preheader (oculto) -->
  <div style=""display:none;max-height:0;overflow:hidden;opacity:0;"">
    Usa este enlace para restablecer tu contraseña. Expira en 1 hora.
  </div>

  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f0f9f4;padding:24px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px;background:#ffffff;border-radius:12px;overflow:hidden;box-shadow:0 10px 25px rgba(0,0,0,.08)"" class=""card"">
          
          <!-- Header EcoWaste -->
          <tr>
            <td style=""background:#16a34a;padding:16px 24px;"">
              <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                <tr>
                  <td width=""40"">
                    <div style=""height:36px;width:36px;border-radius:9999px;background:rgba(255,255,255,.2);display:flex;align-items:center;justify-content:center;font-size:18px;color:#fff"">♻️</div>
                  </td>
                  <td>
                    <div style=""font:600 18px/1.2 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#000"">EcoWaste</div>
                  </td>
                </tr>
              </table>
            </td>
          </tr>

          <!-- Contenido -->
          <tr>
            <td style=""padding:24px 24px 8px;font:400 14px/1.6 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#111827"" class=""text"">
              <h1 style=""margin:0 0 8px;font:700 22px/1.3 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#111827"">Restablecer tu contraseña</h1>
              <p style=""margin:0 0 12px"">Hola {(string.IsNullOrWhiteSpace(user?.Nombre) ? "usuario" : user!.Nombre)},</p>
              <p style=""margin:0 0 16px"">
                Recibimos una solicitud para restablecer tu contraseña. Haz clic en el botón para continuar.
              </p>
              <div style=""margin:24px 0"">
                <a href=""{resetUrl}"" target=""_blank""
                   style=""display:inline-block;background:#16a34a;color:#ffffff;text-decoration:none;
                          padding:12px 20px;border-radius:10px;font:600 14px/1 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif"">
                  Restablecer contraseña
                </a>
              </div>
              <p style=""margin:0 0 10px;font-size:13px;color:#374151"">
                Si el botón no funciona, copia y pega este enlace en tu navegador:
              </p>
              <p style=""margin:0 0 16px;word-break:break-all"">
                <a href=""{resetUrl}"" style=""color:#16a34a;text-decoration:underline"">{resetUrl}</a>
              </p>
              <p style=""margin:0 0 6px;font-size:13px;color:#374151"">Este enlace expira en <strong>1 hora</strong>.</p>
              <p style=""margin:0 0 0;font-size:13px;color:#6b7280"" class=""muted"">
                Si no solicitaste este cambio, puedes ignorar este mensaje: tu contraseña seguirá siendo la misma.
              </p>
            </td>
          </tr>

          <!-- Footer -->
          <tr>
            <td style=""padding:16px 24px;border-top:1px solid #e5e7eb;font:400 12px/1.6 system-ui,-apple-system,Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#6b7280"" class=""footer"">
              © {DateTime.UtcNow:yyyy} EcoWaste ·
              <a href=""{siteUrl}"" style=""color:#16a34a;text-decoration:none"">Visitar sitio</a> ·
              Soporte: <a href=""mailto:soporte@ecowaste.local"" style=""color:#16a34a;text-decoration:none"">soporte@ecowaste.local</a>
            </td>
          </tr>

        </table>
      </td>
    </tr>
  </table>
</body>
</html>";

            var sent = true;
            try { await email.SendAsync(user.Email, subject, html); }
            catch (Exception ex)
            {
                sent = false;
                _logger.LogError(ex, "SMTP error enviando a {Email}", user.Email);
            }

            if (env.IsDevelopment())
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
