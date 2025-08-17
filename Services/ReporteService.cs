using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.DTOs;
using System.Globalization;

namespace RecoleccionResiduosApi.Services
{
    public class ReporteService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ReporteService> _logger;

        public ReporteService(AppDbContext context, ILogger<ReporteService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<DashboardDto> GetDashboardAsync()
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);

                var dashboard = new DashboardDto
                {
                    // ===== lo tuyo existente =====
                    TotalUsuarios = await _context.Usuarios.CountAsync(u => u.Activo),
                    TotalRecolecciones = await _context.Recolecciones.CountAsync(),
                    TotalPuntosOtorgados = await _context.Recolecciones.SumAsync(r => r.PuntosGanados),
                    TotalCanjes = await _context.Canjes.CountAsync(),
                    RecoleccionesMesActual = await _context.Recolecciones.CountAsync(r => r.Fecha >= inicioMes),
                    PuntosMesActual = await _context.Recolecciones.Where(r => r.Fecha >= inicioMes).SumAsync(r => r.PuntosGanados),
                    CanjesMesActual = await _context.Canjes.CountAsync(c => c.FechaCanje >= inicioMes),
                    PesoTotalKg = await _context.Recolecciones.Where(r => r.PesoKg.HasValue).SumAsync(r => r.PesoKg!.Value),

                    TopUsuariosPuntos = await _context.Usuarios
                        .Where(u => u.Activo && u.Puntos > 0)
                        .OrderByDescending(u => u.Puntos)
                        .Take(5)
                        .Select(u => new UsuarioRankingDto
                        {
                            Id = u.Id,
                            Nombre = u.Nombre,
                            Puntos = u.Puntos,
                            TotalRecolecciones = u.Recolecciones.Count()
                        })
                        .ToListAsync(),

                    RecoleccionesPorTipo = await _context.Recolecciones
                        .Include(r => r.TipoResiduo)
                        .GroupBy(r => r.TipoResiduo!.Nombre)
                        .Select(g => new EstadisticaTipoResiduoDto
                        {
                            TipoResiduo = g.Key,
                            Cantidad = g.Count(),
                            PesoTotal = g.Sum(r => r.PesoKg ?? 0),
                            PuntosTotal = g.Sum(r => r.PuntosGanados)
                        })
                        .ToListAsync(),

                    RecoleccionesPorLocalidad = await _context.Recolecciones
                        .Include(r => r.Usuario).ThenInclude(u => u!.Localidad)
                        .Where(r => r.Usuario!.Localidad != null)
                        .GroupBy(r => r.Usuario!.Localidad!.Nombre)
                        .Select(g => new EstadisticaLocalidadDto
                        {
                            Localidad = g.Key,
                            Cantidad = g.Count(),
                            PesoTotal = g.Sum(r => r.PesoKg ?? 0),
                            UsuariosActivos = g.Select(r => r.UsuarioId).Distinct().Count()
                        })
                        .ToListAsync(),

                    TendenciaMensual = await GetTendenciaMensualAsync(6)
                };

                // ===== NUEVO: mapear TendenciaMensual -> TendenciaRecolecciones (lo que usa tu FE)
                dashboard.TendenciaRecolecciones = dashboard.TendenciaMensual
             .OrderBy(t => t.Ano).ThenBy(t => t.Mes)
             .Select(t => new TendenciaItemDto
             {
                 Mes = new DateTime(t.Ano, t.Mes, 1).ToString("MMM", new CultureInfo("es-ES")),
                 Cantidad = t.Cantidad
             })
             .ToList();

                // ===== NUEVO: Top tipos con porcentaje (lo que usa tu FE)
                var totalTop = dashboard.RecoleccionesPorTipo.Sum(x => x.Cantidad);
                dashboard.TiposResiduoMasRecolectados = dashboard.RecoleccionesPorTipo
                    .OrderByDescending(x => x.Cantidad)
                    .Take(5)
                    .Select(x => new TipoResiduoTopDto
                    {
                        Tipo = x.TipoResiduo,
                        Cantidad = x.Cantidad,
                        Porcentaje = totalTop == 0 ? 0 : Math.Round(100.0 * x.Cantidad / totalTop, 2)
                    })
                    .ToList();

                // ===== NUEVO: Actividad reciente (3 listas)
                var recoleccionesRecientes = await _context.Recolecciones
                    .Include(r => r.Usuario)
                    .Include(r => r.TipoResiduo)
                    .OrderByDescending(r => r.Fecha)
                    .Take(5)
                    .Select(r => new RecoleccionRecienteDto
                    {
                        Usuario = r.Usuario!.Nombre,
                        TipoResiduo = r.TipoResiduo!.Nombre,
                        Fecha = r.Fecha
                    })
                    .ToListAsync();

                var nuevosUsuarios = await _context.Usuarios
                    .OrderByDescending(u => u.FechaRegistro)
                    .Take(5)
                    .Select(u => new UsuarioNuevoDto
                    {
                        Nombre = u.Nombre,
                        FechaRegistro = u.FechaRegistro
                    })
                    .ToListAsync();

                var canjesRecientes = await _context.Canjes
                    .Include(c => c.Usuario)
                    .Include(c => c.Descuento)
                    .OrderByDescending(c => c.FechaCanje)
                    .Take(5)
                    .Select(c => new CanjeRecienteDto
                    {
                        Usuario = c.Usuario!.Nombre,
                        Descuento = c.Descuento!.Nombre,
                        Fecha = c.FechaCanje
                    })
                    .ToListAsync();

                dashboard.ActividadReciente = new ActividadRecienteDto
                {
                    RecoleccionesRecientes = recoleccionesRecientes,
                    NuevosUsuarios = nuevosUsuarios,
                    CanjesRecientes = canjesRecientes
                };

                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando dashboard");
                throw;
            }
        }


        public async Task<ReporteUsuarioDto> GetReporteUsuarioAsync(int usuarioId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado");

            var query = _context.Recolecciones
                .Include(r => r.TipoResiduo)
                .Include(r => r.SubtipoResiduo)
                .Where(r => r.UsuarioId == usuarioId);

            if (fechaDesde.HasValue)
                query = query.Where(r => r.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(r => r.Fecha <= fechaHasta.Value);

            var recolecciones = await query.ToListAsync();

            var canjes = await _context.Canjes
                .Include(c => c.Descuento)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return new ReporteUsuarioDto
            {
                Usuario = new UsuarioReporteDto
                {
                    Id = usuario.Id,
                    Nombre = usuario.Nombre,
                    Email = usuario.Email,
                    Puntos = usuario.Puntos,
                    FechaRegistro = usuario.FechaRegistro,
                    Localidad = usuario.Localidad?.Nombre ?? "Sin asignar"
                },
                TotalRecolecciones = recolecciones.Count,
                PesoTotalKg = recolecciones.Sum(r => r.PesoKg ?? 0),
                PuntosTotales = recolecciones.Sum(r => r.PuntosGanados),
                TotalCanjes = canjes.Count,
                PuntosCanjeados = canjes.Sum(c => c.PuntosUtilizados),
                RecoleccionesPorTipo = recolecciones
                    .GroupBy(r => r.TipoResiduo!.Nombre)
                    .Select(g => new EstadisticaTipoResiduoDto
                    {
                        TipoResiduo = g.Key,
                        Cantidad = g.Count(),
                        PesoTotal = g.Sum(r => r.PesoKg ?? 0),
                        PuntosTotal = g.Sum(r => r.PuntosGanados)
                    })
                    .ToList(),
                HistorialRecolecciones = recolecciones
                    .OrderByDescending(r => r.Fecha)
                    .Take(10)
                    .Select(r => new RecoleccionReporteDto
                    {
                        Id = r.Id,
                        Fecha = r.Fecha,
                        TipoResiduo = r.TipoResiduo!.Nombre,
                        Subtipo = r.SubtipoResiduo?.Nombre,
                        PesoKg = r.PesoKg,
                        PuntosGanados = r.PuntosGanados,
                        Estado = r.Estado
                    })
                    .ToList()
            };
        }

        public async Task<ReporteLocalidadDto> GetReporteLocalidadAsync(int localidadId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var localidad = await _context.Localidades.FindAsync(localidadId);
            if (localidad == null)
                throw new ArgumentException("Localidad no encontrada");

            var usuariosQuery = _context.Usuarios.Where(u => u.LocalidadId == localidadId);
            var usuariosIds = await usuariosQuery.Select(u => u.Id).ToListAsync();

            var recoleccionesQuery = _context.Recolecciones
                .Include(r => r.TipoResiduo)
                .Include(r => r.Usuario)
                .Where(r => usuariosIds.Contains(r.UsuarioId));

            if (fechaDesde.HasValue)
                recoleccionesQuery = recoleccionesQuery.Where(r => r.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                recoleccionesQuery = recoleccionesQuery.Where(r => r.Fecha <= fechaHasta.Value);

            var recolecciones = await recoleccionesQuery.ToListAsync();

            return new ReporteLocalidadDto
            {
                Localidad = new LocalidadReporteDto
                {
                    Id = localidad.Id,
                    Nombre = localidad.Nombre,
                    Ciudad = localidad.Ciudad,
                    Departamento = localidad.Departamento
                },
                TotalUsuarios = await usuariosQuery.CountAsync(u => u.Activo),
                UsuariosActivos = recolecciones.Select(r => r.UsuarioId).Distinct().Count(),
                TotalRecolecciones = recolecciones.Count,
                PesoTotalKg = recolecciones.Sum(r => r.PesoKg ?? 0),
                PuntosTotales = recolecciones.Sum(r => r.PuntosGanados),
                RecoleccionesPorTipo = recolecciones
                    .GroupBy(r => r.TipoResiduo!.Nombre)
                    .Select(g => new EstadisticaTipoResiduoDto
                    {
                        TipoResiduo = g.Key,
                        Cantidad = g.Count(),
                        PesoTotal = g.Sum(r => r.PesoKg ?? 0),
                        PuntosTotal = g.Sum(r => r.PuntosGanados)
                    })
                    .ToList(),
                TopUsuarios = await usuariosQuery
                    .Where(u => u.Activo)
                    .OrderByDescending(u => u.Puntos)
                    .Take(10)
                    .Select(u => new UsuarioRankingDto
                    {
                        Id = u.Id,
                        Nombre = u.Nombre,
                        Puntos = u.Puntos,
                        TotalRecolecciones = u.Recolecciones.Count()
                    })
                    .ToListAsync()
            };
        }

        public async Task<ReporteEmpresaDto> GetReporteEmpresaAsync(int empresaId, DateTime? fechaDesde = null, DateTime? fechaHasta = null)
        {
            var empresa = await _context.EmpresasRecolectoras
                .Include(e => e.Localidad)
                .FirstOrDefaultAsync(e => e.Id == empresaId);

            if (empresa == null)
                throw new ArgumentException("Empresa no encontrada");

            var recoleccionesQuery = _context.Recolecciones
                .Include(r => r.TipoResiduo)
                .Include(r => r.Usuario)
                .Where(r => r.EmpresaRecolectoraId == empresaId);

            if (fechaDesde.HasValue)
                recoleccionesQuery = recoleccionesQuery.Where(r => r.Fecha >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                recoleccionesQuery = recoleccionesQuery.Where(r => r.Fecha <= fechaHasta.Value);

            var recolecciones = await recoleccionesQuery.ToListAsync();

            return new ReporteEmpresaDto
            {
                Empresa = new EmpresaReporteDto
                {
                    Id = empresa.Id,
                    Nombre = empresa.Nombre,
                    Telefono = empresa.Telefono,
                    Email = empresa.Email,
                    Localidad = empresa.Localidad?.Nombre ?? "Sin asignar"
                },
                TotalRecolecciones = recolecciones.Count,
                PesoTotalKg = recolecciones.Sum(r => r.PesoKg ?? 0),
                UsuariosAtendidos = recolecciones.Select(r => r.UsuarioId).Distinct().Count(),
                RecoleccionesPorTipo = recolecciones
                    .GroupBy(r => r.TipoResiduo!.Nombre)
                    .Select(g => new EstadisticaTipoResiduoDto
                    {
                        TipoResiduo = g.Key,
                        Cantidad = g.Count(),
                        PesoTotal = g.Sum(r => r.PesoKg ?? 0),
                        PuntosTotal = g.Sum(r => r.PuntosGanados)
                    })
                    .ToList(),
                RecoleccionesPorMes = recolecciones
                    .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                    .Select(g => new TendenciaMensualDto
                    {
                        Mes = g.Key.Month,
                        Ano = g.Key.Year,
                        Cantidad = g.Count(),
                        PesoTotal = g.Sum(r => r.PesoKg ?? 0)
                    })
                    .OrderBy(t => t.Ano)
                    .ThenBy(t => t.Mes)
                    .ToList()
            };
        }

        private async Task<List<TendenciaMensualDto>> GetTendenciaMensualAsync(int meses)
        {
            var fechaInicio = DateTime.UtcNow.AddMonths(-meses);

            var recolecciones = await _context.Recolecciones
                .Where(r => r.Fecha >= fechaInicio)
                .GroupBy(r => new { r.Fecha.Year, r.Fecha.Month })
                .Select(g => new TendenciaMensualDto
                {
                    Mes = g.Key.Month,
                    Ano = g.Key.Year,
                    Cantidad = g.Count(),
                    PesoTotal = g.Sum(r => r.PesoKg ?? 0)
                })
                .OrderBy(t => t.Ano)
                .ThenBy(t => t.Mes)
                .ToListAsync();

            return recolecciones;
        }

        public async Task<List<EstadisticaGeneralDto>> GetEstadisticasGeneralesAsync()
        {
            var estadisticas = new List<EstadisticaGeneralDto>();

            // Estadísticas por tipo de residuo
            var estadisticasTipos = await _context.Recolecciones
                .Include(r => r.TipoResiduo)
                .GroupBy(r => r.TipoResiduo!.Nombre)
                .Select(g => new EstadisticaGeneralDto
                {
                    Categoria = "Tipo de Residuo",
                    Subcategoria = g.Key,
                    Valor = g.Count(),
                    Descripcion = $"Total de recolecciones de {g.Key}"
                })
                .ToListAsync();

            estadisticas.AddRange(estadisticasTipos);

            // Estadísticas por localidad
            var estadisticasLocalidades = await _context.Recolecciones
                .Include(r => r.Usuario)
                .ThenInclude(u => u!.Localidad)
                .Where(r => r.Usuario!.Localidad != null)
                .GroupBy(r => r.Usuario!.Localidad!.Nombre)
                .Select(g => new EstadisticaGeneralDto
                {
                    Categoria = "Localidad",
                    Subcategoria = g.Key,
                    Valor = g.Count(),
                    Descripcion = $"Total de recolecciones en {g.Key}"
                })
                .ToListAsync();

            estadisticas.AddRange(estadisticasLocalidades);

            return estadisticas;
        }
    }
}
