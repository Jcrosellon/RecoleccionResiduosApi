using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ConfiguracionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("zonas")]
        public async Task<ActionResult<List<ConfiguracionZonaDto>>> GetConfiguracionesZona()
        {
            var zonas = await _context.Set<ConfiguracionZona>()
                .AsNoTracking()
                .Include(c => c.Localidad)
                .Include(c => c.TipoResiduo)
                .ToListAsync();

            // ¡OJO!: EF no traduce TimeSpan.ToString(@"hh\:mm") en SQL, por eso materializamos antes
            var dto = zonas.Select(c => new ConfiguracionZonaDto
            {
                Id = c.Id,
                LocalidadId = c.LocalidadId,
                LocalidadNombre = c.Localidad?.Nombre ?? string.Empty,
                TipoResiduoId = c.TipoResiduoId,
                TipoResiduoNombre = c.TipoResiduo?.Nombre ?? string.Empty,
                FrecuenciaDias = c.FrecuenciaDias,
                PesoMinimoKg = c.PesoMinimoKg,
                PesoMaximoKg = c.PesoMaximoKg,
                HoraInicio = c.HoraInicio.ToString(@"hh\:mm"),
                HoraFin = c.HoraFin.ToString(@"hh\:mm"),
                RequiereValidacionFoto = c.RequiereValidacionFoto,
                Activa = c.Activa
            }).ToList();

            return Ok(dto);
        }

        [HttpPost("zonas")]
        public async Task<ActionResult<ConfiguracionZona>> CrearConfiguracionZona(ConfiguracionZona configuracion)
        {
            _context.Set<ConfiguracionZona>().Add(configuracion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetConfiguracionesZona), new { id = configuracion.Id }, configuracion);
        }

        [HttpGet("reglas")]
        public async Task<ActionResult<List<ReglaValidacion>>> GetReglasValidacion()
        {
            var reglas = await _context.Set<ReglaValidacion>()
                .AsNoTracking()
                .Include(r => r.TipoResiduo)
                .Include(r => r.Localidad)
                .ToListAsync();

            return Ok(reglas);
        }

        [HttpPost("reglas")]
        public async Task<ActionResult<ReglaValidacion>> CrearReglaValidacion(ReglaValidacion regla)
        {
            _context.Set<ReglaValidacion>().Add(regla);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetReglasValidacion), new { id = regla.Id }, regla);
        }

        [HttpPut("reglas/{id}")]
        public async Task<IActionResult> ActualizarReglaValidacion(int id, ReglaValidacion regla)
        {
            if (id != regla.Id) return BadRequest();
            _context.Entry(regla).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
