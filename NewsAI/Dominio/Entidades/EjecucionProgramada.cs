using System.ComponentModel.DataAnnotations;
namespace NewsAI.Dominio.Entidades;

public class EjecucionProgramada
{
    public int Id { get; set; }

    [Required]
    public int ConfiguracionId { get; set; }

    [Required]
    public DateTime FechaEjecucion { get; set; }

    [Required]
    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Ejecutando, Completada, Error

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public int? NoticiasEncontradas { get; set; }

    public int? NoticiasProcessadas { get; set; }

    public bool? EmailEnviado { get; set; }

    [MaxLength(1000)]
    public string MensajeError { get; set; }

    public int? ResumenGeneradoId { get; set; }

    // Propiedades de navegación
    public virtual Configuracion Configuracion { get; set; }

    public virtual ResumenGenerado ResumenGenerado { get; set; }
}