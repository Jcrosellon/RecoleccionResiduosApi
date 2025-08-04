using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TiposResiduoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TiposResiduoController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tipos = await _context.TiposResiduo
                .Select(t => new
                {
                    t.Id,
                    t.Nombre,
                    t.Puntos
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [Authorize(Roles = "Administrador")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] TipoResiduo updated)
        {
            var tipo = await _context.TiposResiduo.FindAsync(id);
            if (tipo == null) return NotFound("Tipo de residuo no encontrado.");

            tipo.Nombre = updated.Nombre.Trim();
            tipo.Puntos = updated.Puntos;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                tipo.Id,
                tipo.Nombre,
                tipo.Puntos,
                mensaje = "Actualizado correctamente"
            });
        }

    }
}
