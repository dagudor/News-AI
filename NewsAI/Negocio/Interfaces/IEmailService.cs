using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Interfaces
{
    public interface IEmailService
    {
        Task<bool> EnviarResumenPorEmailAsync(ResumenGenerado resumen, Configuracion configuracion);
        Task<bool> EnviarEmailTestAsync(string destinatario, string asunto, string contenido);
        Task<bool> EnviarResumenAsync(string emailDestino, string contenidoResumen, Configuracion configuracion, int noticiasRelevantes);
    }
}