using System.ComponentModel.DataAnnotations;
namespace NewsAI.Dominio.Entidades;

public class ConfiguracionUrl
{
    public int Id { get; set; }

    [Required]
    public int ConfiguracionId { get; set; }

    [Required]
    public int UrlConfiableId { get; set; }

    public bool Activa { get; set; } = true;

    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

    // Propiedades de navegación
    public virtual Configuracion Configuracion { get; set; }

    public virtual UrlConfiable UrlConfiable { get; set; }
}