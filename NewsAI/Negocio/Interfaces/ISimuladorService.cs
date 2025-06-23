using NewsAI.API.Controllers;
using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Interfaces;

public interface ISimuladorService
{
    Task<SimuladorResponse> ProcesarNoticiasAsync(SimuladorRequest request);
    Task<List<ResumenGenerado>> ObtenerHistorialResumenesAsync(int usuarioId);
    Task<ResumenGenerado> ObtenerResumenPorIdAsync(int id);
    
}