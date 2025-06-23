using NewsAI.Dominio.Entidades;

namespace NewsAI.Dominio.Repositorios;

public interface ISimuladorRepository
{
    Task<ResumenGenerado> GuardarResumenAsync(ResumenGenerado resumen);
    Task<List<ResumenGenerado>> ObtenerHistorialPorUsuarioAsync(int usuarioId);
    Task<ResumenGenerado> ObtenerResumenPorIdAsync(int id);
    Task<bool> ActualizarEstadoEmailAsync(int resumenId, bool emailEnviado);
    Task<bool> EliminarResumenAsync(int id);
}