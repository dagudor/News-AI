using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Interfaces
{
    public interface INoticiasExtractorService
    {
        Task<List<NoticiaExtraida>> ExtraerNoticiasAsync(string url, int limite);
        Task<List<NoticiaExtraida>> ExtraerNoticiasRSSAsync(string urlRss, int limite);
        Task<List<NoticiaExtraida>> ExtraerNoticiasWebAsync(string urlWeb, int limite);
    }
}