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
    [Authorize]
    public class DescuentosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DescuentosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<DescuentoDto>>> GetDescuentos()
        {
            var ahora = DateTime.UtcNow;
            
            var descuentos = await _context.Descuentos
                .Where(d => d.Activo && d.FechaInicio <= ahora && d.FechaFin >= ahora)
                .Select(d => new DescuentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion,
                    PuntosRequeridos = d.PuntosRequeridos,
                    ValorDescuento = d.ValorDescuento,
                    EsPorcentaje = d.EsPorcentaje,
                    FechaInicio = d.FechaInicio,
                    FechaFin = d.FechaFin,
                    Activo = d.Activo,
                    CantidadDisponible = d.CantidadDisponible,
                    CantidadCanjeada = d.Canjes.Count(),
                    PuedeCanjearse = d.CantidadDisponible == -1 || d.Canjes.Count() < d.CantidadDisponible
                })
                .OrderBy(d => d.PuntosRequeridos)
                .ToListAsync();

            return Ok(descuentos);
        }

        [HttpGet("disponibles/{usuarioId}")]
        public async Task<ActionResult<List<DescuentoDto>>> GetDescuentosDisponibles(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null)
            {
                return NotFound("Usuario no encontrado");
            }

            var ahora = DateTime.UtcNow;
            
            var descuentos = await _context.Descuentos
                .Where(d => d.Activo && 
                           d.FechaInicio <= ahora && 
                           d.FechaFin >= ahora &&
                           d.PuntosRequeridos <= usuario.Puntos)
                .Select(d => new DescuentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion,
                    PuntosRequeridos = d.PuntosRequeridos,
                    ValorDescuento = d.ValorDescuento,
                    EsPorcentaje = d.EsPorcentaje,
                    FechaInicio = d.FechaInicio,
                    FechaFin = d.FechaFin,
                    Activo = d.Activo,
                    CantidadDisponible = d.CantidadDisponible,
                    CantidadCanjeada = d.Canjes.Count(),
                    PuedeCanjearse = d.CantidadDisponible == -1 || d.Canjes.Count() < d.CantidadDisponible
                })
                .OrderBy(d => d.PuntosRequeridos)
                .ToListAsync();

            return Ok(descuentos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DescuentoDto>> GetDescuento(int id)
        {
            var descuento = await _context.Descuentos
                .Where(d => d.Id == id)
                .Select(d => new DescuentoDto
                {
                    Id = d.Id,
                    Nombre = d.Nombre,
                    Descripcion = d.Descripcion,
                    PuntosRequeridos = d.PuntosRequeridos,
                    ValorDescuento = d.ValorDescuento,
                    EsPorcentaje = d.EsPorcentaje,
                    FechaInicio = d.FechaInicio,
                    FechaFin = d.FechaFin,
                    Activo = d.Activo,
                    CantidadDisponible = d.CantidadDisponible,
                    CantidadCanjeada = d.Canjes.Count(),
                    PuedeCanjearse = d.CantidadDisponible == -1 || d.Canjes.Count() < d.CantidadDisponible
                })
                .FirstOrDefaultAsync();

            if (descuento == null)
            {
                return NotFound();
            }

            return Ok(descuento);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<DescuentoDto>> CrearDescuento(CrearDescuentoDto dto)
        {
            var descuento = new Descuento
            {
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                PuntosRequeridos = dto.PuntosRequeridos,
                ValorDescuento = dto.ValorDescuento,
                EsPorcentaje = dto.EsPorcentaje,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin,
                CantidadDisponible = dto.CantidadDisponible,
                Activo = true
            };

            _context.Descuentos.Add(descuento);
            await _context.SaveChangesAsync();

            var descuentoDto = new DescuentoDto
            {
                Id = descuento.Id,
                Nombre = descuento.Nombre,
                Descripcion = descuento.Descripcion,
                PuntosRequeridos = descuento.PuntosRequeridos,
                ValorDescuento = descuento.ValorDescuento,
                EsPorcentaje = descuento.EsPorcentaje,
                FechaInicio = descuento.FechaInicio,
                FechaFin = descuento.FechaFin,
                Activo = descuento.Activo,
                CantidadDisponible = descuento.CantidadDisponible,
                CantidadCanjeada = 0,
                PuedeCanjearse = true
            };

            return CreatedAtAction(nameof(GetDescuento), new { id = descuento.Id }, descuentoDto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarDescuento(int id, ActualizarDescuentoDto dto)
        {
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(dto.Nombre))
                descuento.Nombre = dto.Nombre;
            
            if (!string.IsNullOrEmpty(dto.Descripcion))
                descuento.Descripcion = dto.Descripcion;
            
            if (dto.PuntosRequeridos.HasValue)
                descuento.PuntosRequeridos = dto.PuntosRequeridos.Value;
            
            if (dto.ValorDescuento.HasValue)
                descuento.ValorDescuento = dto.ValorDescuento.Value;
            
            if (dto.EsPorcentaje.HasValue)
                descuento.EsPorcentaje = dto.EsPorcentaje.Value;
            
            if (dto.FechaInicio.HasValue)
                descuento.FechaInicio = dto.FechaInicio.Value;
            
            if (dto.FechaFin.HasValue)
                descuento.FechaFin = dto.FechaFin.Value;
            
            if (dto.Activo.HasValue)
                descuento.Activo = dto.Activo.Value;
            
            if (dto.CantidadDisponible.HasValue)
                descuento.CantidadDisponible = dto.CantidadDisponible.Value;

            _context.Descuentos.Update(descuento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarDescuento(int id)
        {
            var descuento = await _context.Descuentos.FindAsync(id);
            if (descuento == null)
            {
                return NotFound();
            }

            // Soft delete - marcar como inactivo en lugar de eliminar
            descuento.Activo = false;
            _context.Descuentos.Update(descuento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
