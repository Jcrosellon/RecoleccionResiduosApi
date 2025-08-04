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

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly string _jwtKey;

        public UsuariosController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _jwtKey = config["Jwt:Key"]!;
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
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return Ok(new { usuario.Id, usuario.Email, usuario.Rol });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest login)
        {
            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == login.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(login.Password, user.PasswordHash))
                return Unauthorized("Credenciales inválidas.");

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

            return Ok(new
            {
                user.Id,
                user.Nombre,
                user.Email,
                user.Rol,
                user.Puntos
            });
        }
    }
}
