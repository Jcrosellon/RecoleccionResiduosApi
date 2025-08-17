using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;
using RecoleccionResiduosApi.Services;

namespace RecoleccionResiduosApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CanjesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly CanjeService _canjeService;

        public CanjesController(AppDbContext context, CanjeService canjeService)
        {
            _context = context;
            _canjeService = canjeService;
        }

        [HttpPost("realizar")]
        public async Task<ActionResult> RealizarCanje(RealizarCanjeDto dto)
        {
            // Obtener usuario del token JWT (simplificado)
            var usuarioIdClaim = User.FindFirst("UsuarioId")?.Value;
            if (!int.TryParse(usuarioIdClaim, out int usuarioId))
            {
                return Unauthorized("Token inválido");
            }

            var resultado = await _canjeService.RealizarCanjeAsync(usuarioId, dto.DescuentoId);

            if (resultado.Exito)
            {
                var canjeDto = new CanjeDto
                {
                    Id = resultado.Canje!.Id,
                    FechaCanje = resultado.Canje.FechaCanje,
                    PuntosUtilizados = resultado.Canje.PuntosUtilizados,
                    CodigoCanje = resultado.Canje.CodigoCanje,
                    Utilizado = resultado.Canje.Utilizado,
                    FechaUtilizacion = resultado.Canje.FechaUtilizacion,
                    UsuarioId = resultado.Canje.UsuarioId
                };

                return Ok(new { mensaje = resultado.Mensaje, canje = canjeDto });
            }
            else
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }
        }

        [HttpPost("utilizar")]
        [AllowAnonymous] // Permitir uso sin autenticación para comercios
        public async Task<ActionResult> UtilizarCanje(UtilizarCanjeDto dto)
        {
            var resultado = await _canjeService.UtilizarCanjeAsync(dto.CodigoCanje);

            if (resultado.Exito)
            {
                return Ok(new { mensaje = resultado.Mensaje });
            }
            else
            {
                return BadRequest(new { mensaje = resultado.Mensaje });
            }
        }

        [HttpGet("validar/{codigoCanje}")]
        [AllowAnonymous] // Permitir validación sin autenticación para comercios
        public async Task<ActionResult<CanjeDto>> ValidarCanje(string codigoCanje)
        {
            var canje = await _canjeService.GetCanjePorCodigoAsync(codigoCanje);

            if (canje == null)
            {
                return NotFound(new { mensaje = "Código de canje no válido" });
            }

            if (canje.Utilizado)
            {
                return BadRequest(new { mensaje = "Este código ya ha sido utilizado" });
            }

            if (DateTime.UtcNow > canje.Descuento!.FechaFin)
            {
                return BadRequest(new { mensaje = "El descuento ha expirado" });
            }

            var canjeDto = new CanjeDto
            {
                Id = canje.Id,
                FechaCanje = canje.FechaCanje,
                PuntosUtilizados = canje.PuntosUtilizados,
                CodigoCanje = canje.CodigoCanje,
                Utilizado = canje.Utilizado,
                FechaUtilizacion = canje.FechaUtilizacion,
                UsuarioId = canje.UsuarioId,
                NombreUsuario = canje.Usuario!.Nombre,
                Descuento = new DescuentoDto
                {
                    Id = canje.Descuento.Id,
                    Nombre = canje.Descuento.Nombre,
                    Descripcion = canje.Descuento.Descripcion,
                    ValorDescuento = canje.Descuento.ValorDescuento,
                    EsPorcentaje = canje.Descuento.EsPorcentaje
                }
            };

            return Ok(canjeDto);
        }

        [HttpGet("usuario/{usuarioId}")]
        public async Task<ActionResult<List<CanjeDto>>> GetCanjesPorUsuario(int usuarioId)
        {
            var canjes = await _context.Canjes
                .Include(c => c.Descuento)
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaCanje)
                .Select(c => new CanjeDto
                {
                    Id = c.Id,
                    FechaCanje = c.FechaCanje,
                    PuntosUtilizados = c.PuntosUtilizados,
                    CodigoCanje = c.CodigoCanje,
                    Utilizado = c.Utilizado,
                    FechaUtilizacion = c.FechaUtilizacion,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.Usuario!.Nombre,
                    Descuento = new DescuentoDto
                    {
                        Id = c.Descuento!.Id,
                        Nombre = c.Descuento.Nombre,
                        Descripcion = c.Descuento.Descripcion,
                        ValorDescuento = c.Descuento.ValorDescuento,
                        EsPorcentaje = c.Descuento.EsPorcentaje,
                        FechaFin = c.Descuento.FechaFin
                    }
                })
                .ToListAsync();

            return Ok(canjes);
        }

        [HttpGet("historial")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<List<CanjeDto>>> GetHistorialCanjes(
            [FromQuery] int? usuarioId = null,
            [FromQuery] bool? utilizado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            var query = _context.Canjes
                .Include(c => c.Descuento)
                .Include(c => c.Usuario)
                .AsQueryable();

            if (usuarioId.HasValue)
                query = query.Where(c => c.UsuarioId == usuarioId.Value);

            if (utilizado.HasValue)
                query = query.Where(c => c.Utilizado == utilizado.Value);

            if (fechaDesde.HasValue)
                query = query.Where(c => c.FechaCanje >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(c => c.FechaCanje <= fechaHasta.Value);

            var canjes = await query
                .OrderByDescending(c => c.FechaCanje)
                .Select(c => new CanjeDto
                {
                    Id = c.Id,
                    FechaCanje = c.FechaCanje,
                    PuntosUtilizados = c.PuntosUtilizados,
                    CodigoCanje = c.CodigoCanje,
                    Utilizado = c.Utilizado,
                    FechaUtilizacion = c.FechaUtilizacion,
                    UsuarioId = c.UsuarioId,
                    NombreUsuario = c.Usuario!.Nombre,
                    Descuento = new DescuentoDto
                    {
                        Id = c.Descuento!.Id,
                        Nombre = c.Descuento.Nombre,
                        Descripcion = c.Descuento.Descripcion,
                        ValorDescuento = c.Descuento.ValorDescuento,
                        EsPorcentaje = c.Descuento.EsPorcentaje
                    }
                })
                .ToListAsync();

            return Ok(canjes);
        }
    }
}
