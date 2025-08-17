namespace RecoleccionResiduosApi.Services
{
    public interface IWhatsAppService
    {
        Task<bool> EnviarMensajeAsync(string numeroTelefono, string mensaje);
        Task<bool> EnviarMensajeTemplateAsync(string numeroTelefono, string templateName, Dictionary<string, string> parametros);
    }
}
