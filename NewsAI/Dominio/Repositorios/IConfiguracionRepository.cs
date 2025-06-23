using NewsAI.Dominio.Entidades;

namespace NewsAI.Dominio.Repositorios
{
    public interface IConfiguracionRepository
    {
        Task<Configuracion> CrearAsync(Configuracion configuracion);
        Task<List<Configuracion>> ObtenerPorUsuarioIdAsync(int usuarioId);
        Task<Configuracion> GuardarAsync(Configuracion configuracion);
        Task<Configuracion> ObtenerPorIdAsync(int id);
        Task<Configuracion> ActualizarAsync(Configuracion configuracion);
        Task<List<Configuracion>> ObtenerActivasParaEjecucionAsync();
        Task<bool> ActualizarUltimaEjecucionAsync(int configuracionId, DateTime ultimaEjecucion);
        Task<Configuracion> ObtenerConUrlsAsync(int configuracionId);
        Task<bool> ActualizarProximaEjecucionAsync(int configuracionId, DateTime? proximaEjecucion);
        Task<List<Configuracion>> ObtenerActivasAsync();
        
    }
}