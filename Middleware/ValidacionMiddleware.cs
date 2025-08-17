using System.Text.Json;

namespace RecoleccionResiduosApi.Middleware
{
    public class ValidacionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ValidacionMiddleware> _logger;

        public ValidacionMiddleware(RequestDelegate next, ILogger<ValidacionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Validar headers requeridos para ciertas rutas
                if (context.Request.Path.StartsWithSegments("/api/recolecciones") && 
                    context.Request.Method == "POST")
                {
                    if (!context.Request.Headers.ContainsKey("Content-Type") ||
                        !context.Request.Headers["Content-Type"].ToString().Contains("application/json"))
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync(JsonSerializer.Serialize(new 
                        { 
                            mensaje = "Content-Type debe ser application/json" 
                        }));
                        return;
                    }
                }

                // Validar tamaño del payload
                if (context.Request.ContentLength > 10 * 1024 * 1024) // 10MB
                {
                    context.Response.StatusCode = 413;
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new 
                    { 
                        mensaje = "El tamaño del payload excede el límite permitido" 
                    }));
                    return;
                }

                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en middleware de validación");
                
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync(JsonSerializer.Serialize(new 
                { 
                    mensaje = "Error interno del servidor" 
                }));
            }
        }
    }
}
