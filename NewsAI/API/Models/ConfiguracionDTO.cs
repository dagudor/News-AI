namespace NewsAI.API.Models;

public class ConfiguracionDTO
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Hashtags { get; set; }
    public string GradoDesarrolloResumen { get; set; }
    public string Lenguaje { get; set; }
    public bool Email { get; set; }
    public bool Audio { get; set; }
    public bool Activa { get; set; }
    public string Frecuencia { get; set; } = "diaria";
    public TimeSpan HoraEnvio { get; set; } = new TimeSpan(8, 0, 0);
    public string DiasPersonalizados { get; set; } = "1,2,3,4,5";
    public DateTime? UltimaEjecucion { get; set; }
    public DateTime? ProximaEjecucion { get; set; }
}