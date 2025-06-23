using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;
using NewsAI.Dominio.Repositorios;

namespace NewsAI.Infraestructura.Persistencia.Repositorios
{
    public class UrlConfiableRepository : IUrlConfiableRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<UrlConfiableRepository> logger;

        public UrlConfiableRepository(AppDbContext context, ILogger<UrlConfiableRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<List<UrlConfiable>> ObtenerPorUsuarioAsync(int usuarioId)
        {
            try
            {
                return await context.UrlsConfiables
                    .Where(u => u.UsuarioId == usuarioId)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URLs para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<List<UrlConfiable>> ObtenerActivasPorUsuarioAsync(int usuarioId)
        {
            try
            {
                return await context.UrlsConfiables
                    .Where(u => u.UsuarioId == usuarioId && u.Activa)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URLs activas para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<UrlConfiable> ObtenerPorIdAsync(int id)
        {

            try
            {
                return await context.UrlsConfiables
                    .FirstOrDefaultAsync(u => u.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URL por ID {id}");
                throw;
            }
        }

        public async Task<UrlConfiable> CrearAsync(UrlConfiable urlConfiable)
        {
            try
            {
                logger.LogInformation($"Creando URL: {urlConfiable.Url} para usuario {urlConfiable.UsuarioId}");

                context.UrlsConfiables.Add(urlConfiable);
                await context.SaveChangesAsync();

                logger.LogInformation($"URL creada con ID {urlConfiable.Id}");
                return urlConfiable;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creando URL: {urlConfiable.Url}");
                throw;
            }
        }

        public async Task<UrlConfiable> ActualizarAsync(UrlConfiable urlConfiable)
        {
            try
            {
                context.UrlsConfiables.Update(urlConfiable);
                await context.SaveChangesAsync();

                logger.LogInformation($"URL {urlConfiable.Id} actualizada");
                return urlConfiable;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error actualizando URL {urlConfiable.Id}");
                throw;
            }
        }

        public async Task<bool> EliminarAsync(int id)
        {
            try
            {
                logger.LogInformation($"Eliminando URL con ID: {id}");

                var url = await context.UrlsConfiables
                    .Include(u => u.ConfiguracionUrls) // Incluir relaciones
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (url == null)
                {
                    logger.LogWarning($"URL con ID {id} no encontrada");
                    return false;
                }

                // EF Core eliminará automáticamente las relaciones en ConfiguracionUrls
                // si están configuradas con DeleteBehavior.Cascade
                context.UrlsConfiables.Remove(url);
                await context.SaveChangesAsync();

                logger.LogInformation($"URL {id} eliminada exitosamente");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error eliminando URL {id}");
                throw;
            }
        }

        public async Task<bool> ExisteUrlParaUsuarioAsync(int usuarioId, string url)
        {
            try
            {
                return await context.UrlsConfiables
                    .AnyAsync(u => u.UsuarioId == usuarioId && u.Url == url);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error verificando existencia de URL para usuario {usuarioId}");
                throw;
            }
        }

        public async Task ActualizarEstadisticasExtraccionAsync(int id, bool exitosa)
        {
            try
            {
                var url = await context.UrlsConfiables.FindAsync(id);
                if (url != null)
                {
                    url.UltimaExtraccion = DateTime.UtcNow;

                    if (exitosa)
                        url.ExtraccionesExitosas++;
                    else
                        url.ExtraccionesFallidas++;

                    await context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error actualizando estadísticas para URL {id}");
                throw;
            }
        }

        public async Task<List<UrlConfiable>> ObtenerPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                return await context.ConfiguracionUrls
                    .Where(cu => cu.ConfiguracionId == configuracionId && cu.Activa)
                    .Include(cu => cu.UrlConfiable)
                    .Select(cu => cu.UrlConfiable)
                    .Where(u => u.Activa)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URLs para configuración {configuracionId}");
                throw;
            }
        }

        //  AÑADIR ESTOS MÉTODOS A UrlConfiableRepository.cs

        public async Task<List<UrlConfiable>> ObtenerConConfiguracionesPorUsuarioAsync(int usuarioId)
        {
            try
            {
                return await context.UrlsConfiables
                    .Include(u => u.ConfiguracionUrls)
                        .ThenInclude(cu => cu.Configuracion)
                    .Where(u => u.UsuarioId == usuarioId)
                    .OrderBy(u => u.Nombre)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URLs con configuraciones para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<UrlConfiable> ObtenerPorUrlUsuarioAsync(int usuarioId, string url)
        {
            try
            {
                return await context.UrlsConfiables
                    .FirstOrDefaultAsync(u => u.UsuarioId == usuarioId && u.Url == url);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error buscando URL existente para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<bool> ExisteUrlConfiguracionAsync(int usuarioId, string url, int configuracionId)
        {
            try
            {
                return await context.UrlsConfiables
                    .Where(u => u.UsuarioId == usuarioId && u.Url == url)
                    .SelectMany(u => u.ConfiguracionUrls)
                    .AnyAsync(cu => cu.ConfiguracionId == configuracionId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error verificando combinación URL-Configuración");
                throw;
            }
        }

        public async Task<UrlConfiable> ObtenerConConfiguracionesAsync(int urlId)
        {
            try
            {
                return await context.UrlsConfiables
                    .Include(u => u.ConfiguracionUrls)
                        .ThenInclude(cu => cu.Configuracion)
                    .FirstOrDefaultAsync(u => u.Id == urlId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo URL {urlId} con configuraciones");
                throw;
            }
        }

        public async Task<bool> EliminarAsociacionAsync(int urlId, int configuracionId)
        {
            try
            {
                var asociacion = await context.ConfiguracionUrls
                    .FirstOrDefaultAsync(cu => cu.UrlConfiableId == urlId && cu.ConfiguracionId == configuracionId);

                if (asociacion == null)
                {
                    return false;
                }

                context.ConfiguracionUrls.Remove(asociacion);
                await context.SaveChangesAsync();

                logger.LogInformation($"Asociación eliminada: URL {urlId} - Config {configuracionId}");
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error eliminando asociación URL {urlId} - Config {configuracionId}");
                throw;
            }
        }
    }
}