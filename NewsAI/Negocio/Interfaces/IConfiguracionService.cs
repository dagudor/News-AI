using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Interfaces
{
    public interface IConfiguracionService
    {
        Task<List<Configuracion>> ObtenerConfiguracionesAsync(int usuarioId);
        Task<Configuracion> ObtenerPorIdAsync(int id); 
        Task<Configuracion> GuardarConfiguracionAsync(Configuracion configuracion);
        Task<Configuracion> CrearConfiguracionAsync(Configuracion configuracion);
        Task<Configuracion> ActualizarConfiguracionAsync(Configuracion configuracion);
    }
}