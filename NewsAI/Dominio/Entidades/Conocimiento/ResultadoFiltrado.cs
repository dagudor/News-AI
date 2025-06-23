namespace NewsAI.Dominio.Entidades.Conocimiento
{
    public class ResultadoFiltrado
    {
        public bool EsRelevante { get; set; }
        public double ScoreCoincidencia { get; set; }
        public List<string> HashtagsCoincidentes { get; set; } = new();
        public List<string> PalabrasCoincidentes { get; set; } = new();
        public string MotivoRelevancia { get; set; } = string.Empty;
        public string MotivoDescarte { get; set; } = string.Empty;
        public List<string> ContextosRelacionados { get; set; } = new();
    }
}