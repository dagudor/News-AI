using System.ComponentModel.DataAnnotations;
namespace NewsAI.Dominio.Entidades;

public class UrlConfiable
{
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [Required]
    [MaxLength(500)]
    public string Url { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nombre { get; set; } // Ej: "El País - Deportes"

    [MaxLength(200)]
    public string Descripcion { get; set; }

    [Required]
    [MaxLength(50)]
    public string TipoFuente { get; set; } = "RSS"; // RSS, WEB

    public bool Activa { get; set; } = true;

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public DateTime? UltimaExtraccion { get; set; }

    public int ExtraccionesExitosas { get; set; } = 0;

    public int ExtraccionesFallidas { get; set; } = 0;

    // Propiedades de navegación
    public virtual Usuario Usuario { get; set; }

    public virtual ICollection<ConfiguracionUrl> ConfiguracionUrls { get; set; } = new List<ConfiguracionUrl>();
}