using Microsoft.AspNetCore.Mvc;
using NewsAI.API.Models;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IUsuarioService usuarioService;

        public UsuarioController(AppDbContext context, IUsuarioService usuarioService)
        {
            this.context = context;
            this.usuarioService = usuarioService;
        }

        //TODO: Implementar esta funcionalidad que está ahoira delegada en el authController
        [HttpPost]
        public async Task<Usuario> CrearUsuario([FromBody] UsuarioDTO usuario)
        {
            return null;
        }

        [HttpGet("{id}")]
        public async Task<Usuario> ObtenerUsuario(int id)
        {
            return null;
        }
    }
}
