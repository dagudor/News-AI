using AutoMapper;
using EntidadConfiguracion = NewsAI.Dominio.Entidades.Configuracion;
using ConfiguracionDTO = NewsAI.API.Models.ConfiguracionDTO;
namespace NewsAI.API.Mappers;
public class ConfiguracionProfile : Profile
{
    public ConfiguracionProfile()
    {
        CreateMap<ConfiguracionDTO, EntidadConfiguracion>();
        CreateMap<EntidadConfiguracion, ConfiguracionDTO>();
    }
}