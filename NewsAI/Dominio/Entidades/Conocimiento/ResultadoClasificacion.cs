namespace NewsAI.Dominio.Entidades.Conocimiento
{
    public class ResultadoClasificacion
    {
        public string ContextoPrincipal { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public List<string> HashtagsDetectados { get; set; } = new();
        public List<string> PalabrasClaveEncontradas { get; set; } = new();
        public double ScoreRelevancia { get; set; }
        public Dictionary<string, double> ScoresPorContexto { get; set; } = new();
        public string RazonClasificacion { get; set; } = string.Empty;
    }
}