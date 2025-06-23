using NewsAI.Dominio.Entidades;

namespace NewsAI.Dominio.Repositorios
{
    public interface IEjecucionProgramadaRepository
    {
        Task<EjecucionProgramada> CrearAsync(EjecucionProgramada ejecucion);
        Task<EjecucionProgramada> ActualizarAsync(EjecucionProgramada ejecucion);
        Task<List<EjecucionProgramada>> ObtenerPendientesAsync();
        Task<List<EjecucionProgramada>> ObtenerPorConfiguracionAsync(int configuracionId);
        Task<EjecucionProgramada> ObtenerPorIdAsync(int id);
        Task MarcarComoIniciadaAsync(int id);
        Task MarcarComoCompletadaAsync(int id, int? resumenGeneradoId = null);
        Task MarcarComoErrorAsync(int id, string mensajeError);
        Task<List<EjecucionProgramada>> ObtenerHistorialAsync(int usuarioId, int limite = 50);
    }
}