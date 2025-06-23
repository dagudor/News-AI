using Microsoft.SemanticKernel;
using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Interfaces
{
    public interface ISemanticKernelService
    {
        Task<string> GenerarResumenAsync(List<NoticiaExtraida> noticias, Configuracion configuracion);
        Task<string> EjecutarPromptAsync(string prompt);
        Task<string> EjecutarPromptAsync(string prompt, string modelo = "gpt-4o-mini");
        Kernel GetKernel();
    }
}