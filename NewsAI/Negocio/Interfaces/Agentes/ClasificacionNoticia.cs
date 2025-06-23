using NewsAI.API.Controllers;
using NewsAI.Dominio.Entidades;
using static NewsAI.Negocio.Services.Agentes.AgenteCoordinador;

namespace NewsAI.Negocio.Interfaces.Agentes
{
    // Resultados de clasificación
    public class ClasificacionNoticia
    {
        // Propiedades originales (MANTENER)
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public List<string> TemasDetectados { get; set; } = new List<string>();
        public List<string> HashtagsGenerados { get; set; } = new List<string>();
        public double ScoreRelevancia { get; set; } // 0-1
        public string Categoria { get; set; } = string.Empty;
        public string UrlOriginal { get; set; } = string.Empty;

        //  NUEVAS PROPIEDADES para el sistema híbrido
        public double? ScoreFiltrado { get; set; }
        public string? MotivoRelevancia { get; set; }
        public List<string> HashtagsCoincidentes { get; set; } = new();
        public List<string> PalabrasCoincidentes { get; set; } = new();
        public string? MetodoClasificacion { get; set; } // "Local", "IA", "Hibrido"
        public string? ContextoDetectado { get; set; } // Del JsonConocimiento
        public DateTime FechaClasificacion { get; set; } = DateTime.UtcNow;
    }

    //  MANTENER ResultadoFiltrado igual (o mejorar si quieres)
    public class ResultadoFiltrado
    {
        public List<ClasificacionNoticia> NoticiasRelevantes { get; set; } = new List<ClasificacionNoticia>();
        public List<ClasificacionNoticia> NoticiasDescartadas { get; set; } = new List<ClasificacionNoticia>();
        public string RazonFiltrado { get; set; } = string.Empty;
        public double ScoreCoincidencia { get; set; }

        //  NUEVAS: Métricas del filtrado inteligente
        public int TotalAnalizadas { get; set; }
        public string MetodoFiltrado { get; set; } = "Standard"; // "Standard", "Inteligente", "Hibrido"
        public List<string> ContextosRelacionados { get; set; } = new();
    }

    //  INTERFACES se mantienen iguales (compatibilidad total)
    public interface IAgenteClasificador
    {
        Task<ClasificacionNoticia> ClasificarNoticiaAsync(NoticiaExtraida noticia);
        Task<List<ClasificacionNoticia>> ClasificarNoticiasAsync(List<NoticiaExtraida> noticias);
    }

    public interface IAgenteFiltrador
    {
        Task<ResultadoFiltrado> FiltrarNoticiasAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion);
        Task<bool> EsRelevantePorHashtagsAsync(ClasificacionNoticia noticia, string hashtagsUsuario);

        
        // Task<ResultadoFiltrado> FiltrarNoticiasAsync(
        //     List<NoticiaExtraida> noticias,
        //     List<string> hashtagsUsuario,
        //     int? limiteNoticias = null);
    }

    public interface IAgenteResumidor
    {
        Task<string> GenerarResumenAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion);
        Task<string> GenerarTituloResumenAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion);
    }

    public interface IAgenteCoordinador
    {
        Task<SimuladorResponse> ProcesarConfiguracionAsync(Configuracion configuracion, List<UrlConfiable> urls);
        Task<bool> EjecutarConfiguracionProgramadaAsync(int configuracionId);
        Task<ResultadoProcesamiento> ProcesarNoticiasCompletaAsync(string url, Configuracion configuracion, int limite = 10);
    }
}