using NewsAI.Dominio.Entidades.Conocimiento;

namespace NewsAI.Negocio.Interfaces;

public interface IConocimientoBaseService
{
        Task<BaseConocimiento> CargarBaseConocimientoAsync();
        Task<ResultadoClasificacion> ClasificarTextoAsync(string texto, string titulo = "");
        Task<ResultadoFiltrado> FiltrarPorHashtagsAsync(string texto, List<string> hashtagsUsuario);
        Task<List<string>> ObtenerContextosRelacionadosAsync(List<string> hashtags);
        Task<Dictionary<string, double>> CalcularScoresPorContextoAsync(string texto);
        Task<List<string>> ExtraerPalabrasClaveAsync(string texto, string contexto);
}