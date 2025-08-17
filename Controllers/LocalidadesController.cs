using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocalidadesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LocalidadesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<LocalidadDto>>> GetLocalidades()
        {
            var localidades = await _context.Localidades
                .Where(l => l.Activa)
                .Select(l => new LocalidadDto
                {
                    Id = l.Id,
                    Nombre = l.Nombre,
                    Ciudad = l.Ciudad,
                    Departamento = l.Departamento,
                    Activa = l.Activa
                })
                .OrderBy(l => l.Nombre)
                .ToListAsync();

            return Ok(localidades);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LocalidadDto>> GetLocalidad(int id)
        {
            var localidad = await _context.Localidades
                .Where(l => l.Id == id)
                .Select(l => new LocalidadDto
                {
                    Id = l.Id,
                    Nombre = l.Nombre,
                    Ciudad = l.Ciudad,
                    Departamento = l.Departamento,
                    Activa = l.Activa
                })
                .FirstOrDefaultAsync();

            if (localidad == null)
            {
                return NotFound();
            }

            return Ok(localidad);
        }
    }
}
