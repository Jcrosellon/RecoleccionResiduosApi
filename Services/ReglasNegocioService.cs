using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Services
{
    public class ReglasNegocioService
    {
        private readonly AppDbContext _context;
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<ReglasNegocioService> _logger;

        public ReglasNegocioService(
            AppDbContext context, 
            NotificacionService notificacionService, 
            ILogger<ReglasNegocioService> logger)
        {
            _context = context;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        public async Task ProcesarRecoleccionCompletadaAsync(int recoleccionId)
        {
            try
            {
                var recoleccion = await _context.Recolecciones
                    .Include(r => r.Usuario)
                    .Include(r => r.TipoResiduo)
                    .FirstOrDefaultAsync(r => r.Id == recoleccionId);

                if (recoleccion == null) return;

                // Marcar como válida y actualizar puntos del usuario
                recoleccion.EsValida = true;
                recoleccion.Estado = "Recolectado";
                recoleccion.FechaRecoleccion = DateTime.UtcNow;

                // Actualizar puntos del usuario
                recoleccion.Usuario!.Puntos += recoleccion.PuntosGanados;

                await _context.SaveChangesAsync();

                // Enviar notificación de confirmación
                await _notificacionService.NotificarRecoleccionConfirmadaAsync(recoleccionId);

                // Verificar si el usuario alcanzó un hito de puntos
                await VerificarHitosPuntosAsync(recoleccion.Usuario);

                _logger.LogInformation($"Recolección {recoleccionId} procesada exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando recolección {recoleccionId}");
            }
        }

        private async Task VerificarHitosPuntosAsync(Usuario usuario)
        {
            var hitos = new[] { 100, 250, 500, 1000, 2500, 5000 };
            
            foreach (var hito in hitos)
            {
                if (usuario.Puntos >= hito)
                {
                    // Verificar si ya se notificó este hito
                    var yaNotificado = await _context.Notificaciones
                        .AnyAsync(n => n.UsuarioId == usuario.Id && 
                                     n.Mensaje.Contains($"{hito} puntos"));

                    if (!yaNotificado)
                    {
                        await _notificacionService.NotificarPuntosAcumuladosAsync(usuario.Id);
                        break; // Solo notificar el hito más alto alcanzado
                    }
                }
            }
        }

        public async Task<bool> PuedeRealizarRecoleccionAsync(int usuarioId, int tipoResiduoId)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Localidad)
                .FirstOrDefaultAsync(u => u.Id == usuarioId);

            if (usuario == null || !usuario.Activo)
                return false;

            // Verificar configuración de zona
            var configuracion = await _context.Set<ConfiguracionZona>()
                .FirstOrDefaultAsync(c => c.LocalidadId == usuario.LocalidadId && 
                                        c.TipoResiduoId == tipoResiduoId && 
                                        c.Activa);

            if (configuracion != null)
            {
                // Verificar horario
                var horaActual = DateTime.Now.TimeOfDay;
                if (horaActual < configuracion.HoraInicio || horaActual > configuracion.HoraFin)
                    return false;

                // Verificar frecuencia
                var ultimaRecoleccion = await _context.Recolecciones
                    .Where(r => r.UsuarioId == usuarioId && r.TipoResiduoId == tipoResiduoId)
                    .OrderByDescending(r => r.Fecha)
                    .FirstOrDefaultAsync();

                if (ultimaRecoleccion != null)
                {
                    var diasTranscurridos = (DateTime.Now - ultimaRecoleccion.Fecha).TotalDays;
                    if (diasTranscurridos < configuracion.FrecuenciaDias)
                        return false;
                }
            }

            return true;
        }

        public async Task ConfigurarReglasIniciales()
        {
            // Crear reglas de validación iniciales si no existen
            if (!await _context.Set<ReglaValidacion>().AnyAsync())
            {
                var reglas = new List<ReglaValidacion>
                {
                    new ReglaValidacion
                    {
                        Nombre = "Separación Correcta",
                        Descripcion = "Bonus por separar correctamente los residuos por subtipo",
                        TipoRegla = "Separacion",
                        Condicion = "{}",
                        PuntosBonus = 5,
                        Activa = true
                    },
                    new ReglaValidacion
                    {
                        Nombre = "Peso Mínimo Orgánicos",
                        Descripcion = "Bonus por entregar al menos 2kg de residuos orgánicos",
                        TipoRegla = "Peso",
                        Condicion = "{\"pesoMinimo\": 2.0}",
                        PuntosBonus = 3,
                        Activa = true
                    },
                    new ReglaValidacion
                    {
                        Nombre = "Usuario Frecuente",
                        Descripcion = "Bonus por realizar al menos 4 recolecciones al mes",
                        TipoRegla = "Frecuencia",
                        Condicion = "{\"minimoRecolecciones\": 4}",
                        PuntosBonus = 10,
                        Activa = true
                    }
                };

                _context.Set<ReglaValidacion>().AddRange(reglas);
                await _context.SaveChangesAsync();
            }

            // Crear configuraciones de zona iniciales
            if (!await _context.Set<ConfiguracionZona>().AnyAsync())
            {
                var localidades = await _context.Localidades.ToListAsync();
                var tiposResiduo = await _context.TiposResiduo.ToListAsync();

                var configuraciones = new List<ConfiguracionZona>();

                foreach (var localidad in localidades)
                {
                    foreach (var tipo in tiposResiduo)
                    {
                        configuraciones.Add(new ConfiguracionZona
                        {
                            LocalidadId = localidad.Id,
                            TipoResiduoId = tipo.Id,
                            FrecuenciaDias = tipo.Nombre == "Orgánico" ? 3 : 7, // Orgánicos cada 3 días, otros cada 7
                            PesoMinimoKg = 0.5,
                            PesoMaximoKg = tipo.Nombre == "Peligroso" ? 10 : 50,
                            HoraInicio = new TimeSpan(6, 0, 0),
                            HoraFin = new TimeSpan(18, 0, 0),
                            RequiereValidacionFoto = tipo.Nombre == "Peligroso",
                            Activa = true
                        });
                    }
                }

                _context.Set<ConfiguracionZona>().AddRange(configuraciones);
                await _context.SaveChangesAsync();
            }
        }
    }
}
