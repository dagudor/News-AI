using AutoMapper;
using EntidadUsuario = NewsAI.Dominio.Entidades.Usuario;
using UsuarioDTO = NewsAI.API.Models.UsuarioDTO;
namespace NewsAI.API.Mappers
{
    public class UsuarioProfile : Profile
    {
        public UsuarioProfile()
        {
            CreateMap<UsuarioDTO, EntidadUsuario>();
            CreateMap<EntidadUsuario, UsuarioDTO>();
        }
    }
}