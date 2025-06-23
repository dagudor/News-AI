using System.ComponentModel.DataAnnotations;

namespace NewsAI.Dominio.Entidades
{
    public class ResumenGenerado
    {
        public int Id { get; set; }
        
        [Required]
        public int UsuarioId { get; set; }
        
        [Required]
        public int ConfiguracionId { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string UrlOrigen { get; set; }
        
        [Required]
        public string ContenidoResumen { get; set; }
        
        public int NoticiasProcesadas { get; set; }
        
        public DateTime FechaGeneracion { get; set; } = DateTime.UtcNow;
        
        public double TiempoProcesamiento { get; set; }
        
        public bool EmailEnviado { get; set; } = false;
        
        // Propiedades de navegación
        public virtual Usuario Usuario { get; set; }
        public virtual Configuracion Configuracion { get; set; }
    }
}