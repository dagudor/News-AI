using System.ComponentModel.DataAnnotations;

namespace NewsAI.Dominio.Entidades
{
    public class NoticiaExtraida
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string Titulo { get; set; } = string.Empty;
        
        [Required]
        public string Contenido { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Url { get; set; } = string.Empty;
        
        public DateTime FechaPublicacion { get; set; } = DateTime.Now;
        
        //  CAMPOS QUE FALTABAN EN TU ENTIDAD
        [MaxLength(100)]
        public string Fuente { get; set; } = string.Empty;
        
        //  HASHTAGS como propiedad de navegación o serializada
        public virtual List<string> Hashtags { get; set; } = new List<string>();
        
        //  ALTERNATIVA: Si prefieres almacenar como string en BD
        // [MaxLength(1000)]
        // public string HashtagsSerializados { get; set; } = string.Empty;
        
        //  MÉTODO HELPER para conversion si usas string serializado
        // public List<string> GetHashtags()
        // {
        //     return string.IsNullOrEmpty(HashtagsSerializados) 
        //         ? new List<string>() 
        //         : HashtagsSerializados.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        // }
        
        // public void SetHashtags(List<string> hashtags)
        // {
        //     HashtagsSerializados = hashtags?.Any() == true ? string.Join(",", hashtags) : string.Empty;
        // }
        
        //  CAMPOS ADICIONALES ÚTILES
        [MaxLength(50)]
        public string Categoria { get; set; } = string.Empty;
        
        public DateTime FechaExtraccion { get; set; } = DateTime.Now;
        
        [MaxLength(200)]
        public string? Extracto { get; set; }
        
        public bool Procesada { get; set; } = false;
    }
}