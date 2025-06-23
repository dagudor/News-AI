using NewsAI.Dominio.Entidades;

namespace NewsAI.Dominio.Repositorios
{
    public interface IUrlConfiableRepository
    {
        Task<List<UrlConfiable>> ObtenerPorUsuarioAsync(int usuarioId);
        Task<List<UrlConfiable>> ObtenerActivasPorUsuarioAsync(int usuarioId);
        Task<UrlConfiable> ObtenerPorIdAsync(int id);
        Task<UrlConfiable> CrearAsync(UrlConfiable urlConfiable);
        Task<UrlConfiable> ActualizarAsync(UrlConfiable urlConfiable);
        Task<bool> EliminarAsync(int id);
        Task<bool> ExisteUrlParaUsuarioAsync(int usuarioId, string url);
        Task ActualizarEstadisticasExtraccionAsync(int id, bool exitosa);
        Task<List<UrlConfiable>> ObtenerPorConfiguracionAsync(int configuracionId);
        Task<List<UrlConfiable>> ObtenerConConfiguracionesPorUsuarioAsync(int usuarioId);
        Task<UrlConfiable> ObtenerPorUrlUsuarioAsync(int usuarioId, string url);
        Task<bool> ExisteUrlConfiguracionAsync(int usuarioId, string url, int configuracionId);
        Task<UrlConfiable> ObtenerConConfiguracionesAsync(int urlId);
        Task<bool> EliminarAsociacionAsync(int urlId, int configuracionId);
    }
}