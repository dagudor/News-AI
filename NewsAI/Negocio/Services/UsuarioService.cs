using AutoMapper;
using NewsAI.Negocio.Interfaces;
public class Usuario :IUsuarioService
{
    private readonly AppDbContext _context;
    private readonly IMapper mapper;

    public Usuario(AppDbContext context, IMapper mapper)
    {
        _context = context;
        this.mapper = mapper;
    }

    public Task<Usuario> ActualizarUsuarioAsync(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    public Task<Usuario> CrearUsuarioAsync(Usuario usuario)
    {
        throw new NotImplementedException();
    }

    public Task<bool> EliminarUsuarioAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<List<Usuario>> ObtenerTodosLosUsuariosAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Usuario> ObtenerUsuarioAsync(int id)
    {
        throw new NotImplementedException();
    }
}