using System.Text.Json.Serialization;

namespace NewsAI.Dominio.Entidades.Conocimiento
{
    public class BaseConocimiento
    {
        [JsonPropertyName("contextos")]
        public Dictionary<string, ContextoConocimiento> Contextos { get; set; } = new();
    }
}