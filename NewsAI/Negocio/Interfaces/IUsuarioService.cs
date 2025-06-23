namespace NewsAI.Negocio.Interfaces;
public interface IUsuarioService
{
    Task<Usuario> CrearUsuarioAsync(Usuario usuario);
    Task<Usuario> ObtenerUsuarioAsync(int id);
    Task<List<Usuario>> ObtenerTodosLosUsuariosAsync();
    Task<Usuario> ActualizarUsuarioAsync(Usuario usuario);
    Task<bool> EliminarUsuarioAsync(int id);
}