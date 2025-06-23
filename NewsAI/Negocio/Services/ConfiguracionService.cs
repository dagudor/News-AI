using NewsAI.Dominio.Entidades;
using NewsAI.Dominio.Repositorios;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.Negocio.Services
{
    public class ConfiguracionService : IConfiguracionService
    {
        private readonly IConfiguracionRepository configuracionRepository;
        private readonly ILogger<ConfiguracionService> logger;

        public ConfiguracionService(
            IConfiguracionRepository configuracionRepository,
            ILogger<ConfiguracionService> logger)
        {
            this.configuracionRepository = configuracionRepository;
            this.logger = logger;
        }

        public async Task<List<Configuracion>> ObtenerConfiguracionesAsync(int usuarioId)
        {
            try
            {
                logger.LogInformation($"Servicio: Obteniendo configuraciones para usuario {usuarioId}");

                var configuraciones = await configuracionRepository.ObtenerPorUsuarioIdAsync(usuarioId);

                logger.LogInformation($"Servicio: Se encontraron {configuraciones.Count} configuraciones");
                return configuraciones;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en servicio al obtener configuraciones para usuario {usuarioId}");
                throw;
            }
        }

        // MÉTODO FALTANTE AGREGADO
        public async Task<Configuracion> ObtenerPorIdAsync(int id)
        {
            try
            {
                logger.LogInformation($"Servicio: Obteniendo configuración por ID {id}");

                var configuracion = await configuracionRepository.ObtenerPorIdAsync(id);

                if (configuracion != null)
                {
                    logger.LogInformation($"Servicio: Configuración {id} encontrada para usuario {configuracion.UsuarioId}");
                }
                else
                {
                    logger.LogWarning($"Servicio: No se encontró configuración con ID {id}");
                }

                return configuracion;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en servicio al obtener configuración por ID {id}");
                throw;
            }
        }

        public async Task<Configuracion> GuardarConfiguracionAsync(Configuracion configuracion)
        {
            try
            {
                logger.LogInformation($"Servicio: Guardando configuración para usuario {configuracion.UsuarioId}");
                return await configuracionRepository.GuardarAsync(configuracion);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en servicio al guardar configuración");
                throw;
            }
        }

        public async Task<Configuracion> CrearConfiguracionAsync(Configuracion configuracion)
        {
            try
            {
                logger.LogInformation($"Servicio: Creando nueva configuración para usuario {configuracion.UsuarioId}");

                // Validaciones de negocio si las necesitas
                if (string.IsNullOrWhiteSpace(configuracion.Hashtags))
                {
                    throw new ArgumentException("Los hashtags son obligatorios");
                }

                if (string.IsNullOrWhiteSpace(configuracion.GradoDesarrolloResumen))
                {
                    throw new ArgumentException("El grado de desarrollo del resumen es obligatorio");
                }

                var resultado = await configuracionRepository.CrearAsync(configuracion);

                logger.LogInformation($"Servicio: Configuración creada exitosamente con ID {resultado.Id}");
                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en servicio al crear configuración para usuario {configuracion.UsuarioId}");
                throw;
            }
        }

        public async Task<Configuracion> ActualizarConfiguracionAsync(Configuracion configuracion)
        {
            try
            {
                logger.LogInformation($"Servicio: Actualizando configuración {configuracion.Id}");

                var resultado = await configuracionRepository.ActualizarAsync(configuracion);

                if (resultado != null)
                {
                    logger.LogInformation($"Servicio: Configuración {configuracion.Id} actualizada exitosamente");
                }

                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error en servicio al actualizar configuración {configuracion.Id}");
                throw;
            }
        }
    }
}