namespace NewsAI.Dominio.Entidades;
public class Usuario
  {
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public DateTime? FechaAlta { get; set; }

    public List<Configuracion> Configuraciones { get; set; } = new();
  }