using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using RecoleccionResiduosApi.DTOs;


namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecoleccionesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecoleccionesController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("programar")]
        public async Task<IActionResult> Programar([FromBody] Recoleccion reco)
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return NotFound("Usuario no encontrado");

            var tipo = await _context.TiposResiduo.FindAsync(reco.TipoResiduoId);
            if (tipo == null) return BadRequest("Tipo de residuo no válido");

            reco.UsuarioId = usuarioId;
            reco.EsValida = true;
            reco.PuntosGanados = tipo.Puntos;
            usuario.Puntos += tipo.Puntos;

            _context.Recolecciones.Add(reco);
            await _context.SaveChangesAsync();

            var dto = new RecoleccionDto
            {
                Id = reco.Id,
                TipoResiduo = tipo.Nombre,
                Subtipo = reco.Subtipo,
                Fecha = reco.Fecha,
                PesoKg = reco.PesoKg,
                EsValida = reco.EsValida,
                PuntosGanados = reco.PuntosGanados
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("historial")]
        public async Task<IActionResult> Historial()
        {
            var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

            var recolecciones = await _context.Recolecciones
                .Include(r => r.TipoResiduo)
                .Where(r => r.UsuarioId == usuarioId)
                .Select(r => new RecoleccionDto
                {
                    Id = r.Id,
                    TipoResiduo = r.TipoResiduo!.Nombre,
                    Subtipo = r.Subtipo,
                    Fecha = r.Fecha,
                    PesoKg = r.PesoKg,
                    EsValida = r.EsValida,
                    PuntosGanados = r.PuntosGanados
                })
                .ToListAsync();

            return Ok(recolecciones);
        }


    }
}
