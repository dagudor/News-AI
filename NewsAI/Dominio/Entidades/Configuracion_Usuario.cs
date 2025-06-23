namespace NewsAI.Dominio.Entidades;

public class Configuracion_Usuario
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int ConfiguracionId { get; set; }
    
    public Usuario Usuario { get; set; }
    public Configuracion Configuracion { get; set; }
}