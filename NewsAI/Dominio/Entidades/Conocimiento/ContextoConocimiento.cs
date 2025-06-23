using System.Text.Json.Serialization;
public class ContextoConocimiento
{

    [JsonPropertyName("hashtags")]
    public List<string> Hashtags { get; set; } = new();

    [JsonPropertyName("categoria")]
    public string Categoria { get; set; } = string.Empty;

    [JsonPropertyName("palabrasClaves")]
    public List<string> PalabrasClaves { get; set; } = new();

    [JsonPropertyName("pesos")]
    public Dictionary<string, int> Pesos { get; set; } = new();
}