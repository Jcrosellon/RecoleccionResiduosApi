using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Data;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Services
{
    public class CanjeService
    {
        private readonly AppDbContext _context;
        private readonly NotificacionService _notificacionService;
        private readonly ILogger<CanjeService> _logger;

        public CanjeService(AppDbContext context, NotificacionService notificacionService, ILogger<CanjeService> logger)
        {
            _context = context;
            _notificacionService = notificacionService;
            _logger = logger;
        }

        public async Task<(bool Exito, string Mensaje, Canje? Canje)> RealizarCanjeAsync(int usuarioId, int descuentoId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                // Verificar usuario
                var usuario = await _context.Usuarios.FindAsync(usuarioId);
                if (usuario == null)
                {
                    return (false, "Usuario no encontrado", null);
                }

                // Verificar descuento
                var descuento = await _context.Descuentos.FindAsync(descuentoId);
                if (descuento == null)
                {
                    return (false, "Descuento no encontrado", null);
                }

                // Validaciones
                var validacion = ValidarCanje(usuario, descuento);
                if (!validacion.EsValido)
                {
                    return (false, validacion.Mensaje, null);
                }

                // Verificar disponibilidad
                if (descuento.CantidadDisponible > 0)
                {
                    var canjesRealizados = await _context.Canjes
                        .CountAsync(c => c.DescuentoId == descuentoId);
                    
                    if (canjesRealizados >= descuento.CantidadDisponible)
                    {
                        return (false, "Descuento agotado", null);
                    }
                }

                // Crear canje
                var canje = new Canje
                {
                    UsuarioId = usuarioId,
                    DescuentoId = descuentoId,
                    FechaCanje = DateTime.UtcNow,
                    PuntosUtilizados = descuento.PuntosRequeridos,
                    CodigoCanje = GenerarCodigoCanje(),
                    Utilizado = false
                };

                _context.Canjes.Add(canje);

                // Descontar puntos del usuario
                usuario.Puntos -= descuento.PuntosRequeridos;
                _context.Usuarios.Update(usuario);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Enviar notificación
                await EnviarNotificacionCanjeAsync(canje);

                _logger.LogInformation($"Canje realizado exitosamente. Usuario: {usuarioId}, Descuento: {descuentoId}, Código: {canje.CodigoCanje}");

                return (true, "Canje realizado exitosamente", canje);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error realizando canje. Usuario: {usuarioId}, Descuento: {descuentoId}");
                return (false, "Error interno del servidor", null);
            }
        }

        public async Task<(bool Exito, string Mensaje)> UtilizarCanjeAsync(string codigoCanje)
        {
            try
            {
                var canje = await _context.Canjes
                    .Include(c => c.Descuento)
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.CodigoCanje == codigoCanje);

                if (canje == null)
                {
                    return (false, "Código de canje no válido");
                }

                if (canje.Utilizado)
                {
                    return (false, "Este código ya ha sido utilizado");
                }

                // Verificar vigencia del descuento
                if (DateTime.UtcNow > canje.Descuento!.FechaFin)
                {
                    return (false, "El descuento ha expirado");
                }

                // Marcar como utilizado
                canje.Utilizado = true;
                canje.FechaUtilizacion = DateTime.UtcNow;

                _context.Canjes.Update(canje);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Canje utilizado exitosamente. Código: {codigoCanje}");

                return (true, "Canje utilizado exitosamente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error utilizando canje. Código: {codigoCanje}");
                return (false, "Error interno del servidor");
            }
        }

        public async Task<List<Descuento>> GetDescuentosDisponiblesAsync(int usuarioId)
        {
            var usuario = await _context.Usuarios.FindAsync(usuarioId);
            if (usuario == null) return new List<Descuento>();

            var ahora = DateTime.UtcNow;

            return await _context.Descuentos
                .Where(d => d.Activo && 
                           d.FechaInicio <= ahora && 
                           d.FechaFin >= ahora &&
                           d.PuntosRequeridos <= usuario.Puntos)
                .OrderBy(d => d.PuntosRequeridos)
                .ToListAsync();
        }

        public async Task<List<Canje>> GetCanjesPorUsuarioAsync(int usuarioId)
        {
            return await _context.Canjes
                .Include(c => c.Descuento)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.FechaCanje)
                .ToListAsync();
        }

        public async Task<Canje?> GetCanjePorCodigoAsync(string codigoCanje)
        {
            return await _context.Canjes
                .Include(c => c.Descuento)
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.CodigoCanje == codigoCanje);
        }

        private (bool EsValido, string Mensaje) ValidarCanje(Usuario usuario, Descuento descuento)
        {
            if (!usuario.Activo)
                return (false, "Usuario inactivo");

            if (!descuento.Activo)
                return (false, "Descuento no disponible");

            if (DateTime.UtcNow < descuento.FechaInicio)
                return (false, "Descuento aún no disponible");

            if (DateTime.UtcNow > descuento.FechaFin)
                return (false, "Descuento expirado");

            if (usuario.Puntos < descuento.PuntosRequeridos)
                return (false, $"Puntos insuficientes. Necesitas {descuento.PuntosRequeridos} puntos, tienes {usuario.Puntos}");

            return (true, "Válido");
        }

        private string GenerarCodigoCanje()
        {
            // Generar código único de 8 caracteres
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var codigo = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            // Verificar que no exista (muy improbable, pero por seguridad)
            while (_context.Canjes.Any(c => c.CodigoCanje == codigo))
            {
                codigo = new string(Enumerable.Repeat(chars, 8)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }

            return codigo;
        }

        private async Task EnviarNotificacionCanjeAsync(Canje canje)
        {
            try
            {
                var canjeCompleto = await _context.Canjes
                    .Include(c => c.Descuento)
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == canje.Id);

                if (canjeCompleto != null)
                {
                    var mensaje = $"¡Felicitaciones {canjeCompleto.Usuario!.Nombre}! " +
                                 $"Has canjeado exitosamente '{canjeCompleto.Descuento!.Nombre}'. " +
                                 $"Tu código de canje es: {canjeCompleto.CodigoCanje}. " +
                                 $"¡Disfruta tu descuento! 🎉";

                    await _notificacionService.EnviarNotificacionAsync(
                        canjeCompleto.UsuarioId,
                        mensaje,
                        "WhatsApp"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enviando notificación de canje {canje.Id}");
            }
        }
    }
}
