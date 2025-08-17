using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;
using System.Text.Json;

namespace RecoleccionResiduosApi.Services
{
    public class ValidacionService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ValidacionService> _logger;

        public ValidacionService(AppDbContext context, ILogger<ValidacionService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<(bool EsValida, List<string> Errores, int PuntosCalculados)> ValidarRecoleccionAsync(
            int usuarioId, 
            int tipoResiduoId, 
            int? subtipoResiduoId, 
            double? pesoKg, 
            DateTime fechaSolicitud)
        {
            var errores = new List<string>();
            var puntosCalculados = 0;

            try
            {
                // Obtener datos necesarios
                var usuario = await _context.Usuarios
                    .Include(u => u.Localidad)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                var tipoResiduo = await _context.TiposResiduo.FindAsync(tipoResiduoId);
                var subtipoResiduo = subtipoResiduoId.HasValue 
                    ? await _context.SubtiposResiduo.FindAsync(subtipoResiduoId.Value) 
                    : null;

                if (usuario == null)
                {
                    errores.Add("Usuario no encontrado");
                    return (false, errores, 0);
                }

                if (tipoResiduo == null)
                {
                    errores.Add("Tipo de residuo no encontrado");
                    return (false, errores, 0);
                }

                // Validar configuración de zona
                var configuracionZona = await _context.Set<ConfiguracionZona>()
                    .FirstOrDefaultAsync(c => c.LocalidadId == usuario.LocalidadId && 
                                            c.TipoResiduoId == tipoResiduoId && 
                                            c.Activa);

                if (configuracionZona != null)
                {
                    var validacionZona = ValidarConfiguracionZona(configuracionZona, pesoKg, fechaSolicitud);
                    if (!validacionZona.EsValida)
                    {
                        errores.AddRange(validacionZona.Errores);
                    }
                }

                // Validar frecuencia de recolección
                var validacionFrecuencia = await ValidarFrecuenciaRecoleccionAsync(usuarioId, tipoResiduoId, fechaSolicitud);
                if (!validacionFrecuencia.EsValida)
                {
                    errores.AddRange(validacionFrecuencia.Errores);
                }

                // Validar peso
                var validacionPeso = ValidarPeso(pesoKg, tipoResiduo);
                if (!validacionPeso.EsValida)
                {
                    errores.AddRange(validacionPeso.Errores);
                }

                // Calcular puntos si es válida
                if (errores.Count == 0)
                {
                    puntosCalculados = await CalcularPuntosAsync(tipoResiduo, subtipoResiduo, pesoKg, usuario);
                }

                return (errores.Count == 0, errores, puntosCalculados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validando recolección para usuario {usuarioId}");
                errores.Add("Error interno de validación");
                return (false, errores, 0);
            }
        }

        private (bool EsValida, List<string> Errores) ValidarConfiguracionZona(
            ConfiguracionZona configuracion, 
            double? pesoKg, 
            DateTime fechaSolicitud)
        {
            var errores = new List<string>();

            // Validar peso mínimo y máximo
            if (pesoKg.HasValue)
            {
                if (pesoKg.Value < configuracion.PesoMinimoKg)
                {
                    errores.Add($"El peso mínimo para este tipo de residuo en tu zona es {configuracion.PesoMinimoKg}kg");
                }

                if (pesoKg.Value > configuracion.PesoMaximoKg)
                {
                    errores.Add($"El peso máximo para este tipo de residuo en tu zona es {configuracion.PesoMaximoKg}kg");
                }
            }

            // Validar horario
            var horaSolicitud = fechaSolicitud.TimeOfDay;
            if (horaSolicitud < configuracion.HoraInicio || horaSolicitud > configuracion.HoraFin)
            {
                errores.Add($"Las solicitudes para este tipo de residuo solo se aceptan entre {configuracion.HoraInicio:hh\\:mm} y {configuracion.HoraFin:hh\\:mm}");
            }

            return (errores.Count == 0, errores);
        }

        private async Task<(bool EsValida, List<string> Errores)> ValidarFrecuenciaRecoleccionAsync(
            int usuarioId, 
            int tipoResiduoId, 
            DateTime fechaSolicitud)
        {
            var errores = new List<string>();

            // Buscar la última recolección del mismo tipo
            var ultimaRecoleccion = await _context.Recolecciones
                .Where(r => r.UsuarioId == usuarioId && r.TipoResiduoId == tipoResiduoId)
                .OrderByDescending(r => r.Fecha)
                .FirstOrDefaultAsync();

            if (ultimaRecoleccion != null)
            {
                // Obtener configuración de frecuencia (por defecto 7 días)
                var configuracion = await _context.Set<ConfiguracionZona>()
                    .FirstOrDefaultAsync(c => c.TipoResiduoId == tipoResiduoId);

                var frecuenciaDias = configuracion?.FrecuenciaDias ?? 7;
                var diasTranscurridos = (fechaSolicitud - ultimaRecoleccion.Fecha).TotalDays;

                if (diasTranscurridos < frecuenciaDias)
                {
                    var diasRestantes = Math.Ceiling(frecuenciaDias - diasTranscurridos);
                    errores.Add($"Debes esperar {diasRestantes} días más antes de solicitar otra recolección de este tipo");
                }
            }

            return (errores.Count == 0, errores);
        }

        private (bool EsValida, List<string> Errores) ValidarPeso(double? pesoKg, TipoResiduo tipoResiduo)
        {
            var errores = new List<string>();

            if (!pesoKg.HasValue || pesoKg.Value <= 0)
            {
                errores.Add("El peso debe ser mayor a 0");
                return (false, errores);
            }

            // Validaciones específicas por tipo de residuo
            switch (tipoResiduo.Nombre.ToLower())
            {
                case "orgánico":
                    if (pesoKg.Value > 50)
                        errores.Add("El peso máximo para residuos orgánicos es 50kg");
                    break;
                
                case "inorgánico reciclable":
                    if (pesoKg.Value > 30)
                        errores.Add("El peso máximo para residuos inorgánicos reciclables es 30kg");
                    break;
                
                case "peligroso":
                    if (pesoKg.Value > 10)
                        errores.Add("El peso máximo para residuos peligrosos es 10kg");
                    break;
            }

            return (errores.Count == 0, errores);
        }

        private async Task<int> CalcularPuntosAsync(
            TipoResiduo tipoResiduo, 
            SubtipoResiduo? subtipoResiduo, 
            double? pesoKg, 
            Usuario usuario)
        {
            var puntosBase = tipoResiduo.Puntos;
            var puntosSubtipo = subtipoResiduo?.PuntosAdicionales ?? 0;
            var puntosPeso = pesoKg.HasValue ? (int)(pesoKg.Value * 2) : 0; // 2 puntos por kg

            // Aplicar reglas de validación activas
            var reglasActivas = await _context.Set<ReglaValidacion>()
                .Where(r => r.Activa && 
                           (r.TipoResiduoId == null || r.TipoResiduoId == tipoResiduo.Id) &&
                           (r.LocalidadId == null || r.LocalidadId == usuario.LocalidadId))
                .ToListAsync();

            var puntosBonus = 0;
            foreach (var regla in reglasActivas)
            {
                if (await EvaluarReglaAsync(regla, tipoResiduo, subtipoResiduo, pesoKg, usuario))
                {
                    puntosBonus += regla.PuntosBonus;
                }
            }

            return puntosBase + puntosSubtipo + puntosPeso + puntosBonus;
        }

        private async Task<bool> EvaluarReglaAsync(
            ReglaValidacion regla, 
            TipoResiduo tipoResiduo, 
            SubtipoResiduo? subtipoResiduo, 
            double? pesoKg, 
            Usuario usuario)
        {
            try
            {
                // Evaluar condiciones según el tipo de regla
                switch (regla.TipoRegla.ToLower())
                {
                    case "separacion":
                        return subtipoResiduo != null; // Bonus por separar correctamente
                    
                    case "peso":
                        var condicionesPeso = JsonSerializer.Deserialize<Dictionary<string, object>>(regla.Condicion);
                        if (condicionesPeso != null && pesoKg.HasValue)
                        {
                            if (condicionesPeso.ContainsKey("pesoMinimo"))
                            {
                                var pesoMinimo = Convert.ToDouble(condicionesPeso["pesoMinimo"]);
                                return pesoKg.Value >= pesoMinimo;
                            }
                        }
                        break;
                    
                    case "frecuencia":
                        // Bonus por ser usuario frecuente
                        var recoleccionesUltimoMes = await _context.Recolecciones
                            .CountAsync(r => r.UsuarioId == usuario.Id && 
                                           r.Fecha >= DateTime.UtcNow.AddDays(-30));
                        
                        var condicionesFrecuencia = JsonSerializer.Deserialize<Dictionary<string, object>>(regla.Condicion);
                        if (condicionesFrecuencia != null && condicionesFrecuencia.ContainsKey("minimoRecolecciones"))
                        {
                            var minimoRecolecciones = Convert.ToInt32(condicionesFrecuencia["minimoRecolecciones"]);
                            return recoleccionesUltimoMes >= minimoRecolecciones;
                        }
                        break;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error evaluando regla {regla.Id}");
                return false;
            }
        }

        public async Task<bool> ValidarCanjeAsync(int usuarioId, int descuentoId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            var descuento = await _context.Descuentos.FindAsync(descuentoId);

            if (usuario == null || descuento == null)
                return false;

            if (!usuario.Activo || !descuento.Activo)
                return false;

            if (usuario.Puntos < descuento.PuntosRequeridos)
                return false;

            if (DateTime.UtcNow < descuento.FechaInicio || DateTime.UtcNow > descuento.FechaFin)
                return false;

            // Verificar disponibilidad
            if (descuento.CantidadDisponible > 0)
            {
                var canjesRealizados = await _context.Canjes.CountAsync(c => c.DescuentoId == descuentoId);
                if (canjesRealizados >= descuento.CantidadDisponible)
                    return false;
            }

            return true;
        }
    }
}
