using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Repositorios;
using NewsAI.Dominio.Entidades;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UrlsConfiablesController : ControllerBase
    {
        private readonly IUrlConfiableRepository urlRepository;
        private readonly INoticiasExtractorService noticiasExtractor;
        private readonly ILogger<UrlsConfiablesController> logger;
        private readonly AppDbContext context;

        public UrlsConfiablesController(
            IUrlConfiableRepository urlRepository,
            INoticiasExtractorService noticiasExtractor,
            ILogger<UrlsConfiablesController> logger,
            AppDbContext context)
        {
            this.urlRepository = urlRepository;
            this.noticiasExtractor = noticiasExtractor;
            this.logger = logger;
            this.context = context;
        }

        [HttpPost]
        public async Task<ActionResult<object>> Crear([FromBody] CrearUrlRequest request)
        {
            try
            {
                // NORMALIZAR URL ANTES DE GUARDAR
                var urlNormalizada = NormalizarUrl(request.Url);
                logger.LogInformation($"🔧 URL normalizada: '{request.Url}' → '{urlNormalizada}'");

                logger.LogInformation($"Creando URL {urlNormalizada} para configuración {request.ConfiguracionId}");

                // Verificar si la combinación ya existe
                var yaExiste = await context.ConfiguracionUrls
                    .Include(cu => cu.UrlConfiable)
                    .AnyAsync(cu => cu.UrlConfiable.UsuarioId == request.UsuarioId &&
                                   cu.UrlConfiable.Url == urlNormalizada &&
                                   cu.ConfiguracionId == request.ConfiguracionId);

                if (yaExiste)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Esta URL ya está asociada a la configuración seleccionada"
                    });
                }

                // Buscar si la URL normalizada ya existe para este usuario
                var urlExistente = await context.UrlsConfiables
                    .FirstOrDefaultAsync(u => u.UsuarioId == request.UsuarioId && u.Url == urlNormalizada);

                UrlConfiable urlConfiable;

                if (urlExistente != null)
                {
                    logger.LogInformation($"URL ya existe (ID: {urlExistente.Id}), asociando a nueva configuración");
                    urlConfiable = urlExistente;
                }
                else
                {
                    urlConfiable = new UrlConfiable
                    {
                        UsuarioId = request.UsuarioId,
                        Url = urlNormalizada, // USAR URL NORMALIZADA
                        Nombre = request.Nombre ?? ExtraerNombreDeUrl(urlNormalizada),
                        Descripcion = $"Fuente para configuración: {request.ConfiguracionId}",
                        TipoFuente = "RSS",
                        Activa = true,
                        FechaCreacion = DateTime.UtcNow
                    };

                    context.UrlsConfiables.Add(urlConfiable);
                    await context.SaveChangesAsync();
                    logger.LogInformation($"Nueva URL creada con ID: {urlConfiable.Id}");
                }

                // Asociar la URL a la configuración
                context.ConfiguracionUrls.Add(new ConfiguracionUrl
                {
                    ConfiguracionId = request.ConfiguracionId,
                    UrlConfiableId = urlConfiable.Id,
                    Activa = true,
                    FechaAsignacion = DateTime.UtcNow
                });

                await context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = urlExistente != null ?
                        "URL existente asociada a nueva configuración" :
                        "URL creada y asociada correctamente",
                    data = new
                    {
                        id = urlConfiable.Id,
                        nombre = urlConfiable.Nombre,
                        url = urlConfiable.Url,
                        esNueva = urlExistente == null
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creando/asociando URL");
                return StatusCode(500, new { success = false, message = "Error interno" });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<object>> ActualizarUrl(int id, [FromBody] ActualizarUrlRequest request)
        {
            try
            {
                logger.LogInformation($"Actualizando URL {id}");

                var url = await context.UrlsConfiables.FirstOrDefaultAsync(u => u.Id == id);
                if (url == null)
                {
                    return NotFound(new { success = false, message = "URL no encontrada" });
                }

                // NORMALIZAR URL ANTES DE ACTUALIZAR
                var urlNormalizada = NormalizarUrl(request.Url);
                logger.LogInformation($"🔧 URL actualizada normalizada: '{request.Url}' → '{urlNormalizada}'");

                // Verificar si la nueva URL ya existe
                if (url.Url != urlNormalizada)
                {
                    var existeOtra = await context.UrlsConfiables
                        .AnyAsync(u => u.Url == urlNormalizada && u.UsuarioId == url.UsuarioId && u.Id != id);

                    if (existeOtra)
                    {
                        return BadRequest(new { success = false, message = "Ya existe otra URL con esa dirección" });
                    }
                }

                // Actualizar campos
                url.Nombre = request.Nombre ?? url.Nombre;
                url.Url = urlNormalizada; // USAR URL NORMALIZADA
                url.Descripcion = request.Descripcion;
                url.TipoFuente = request.TipoFuente ?? url.TipoFuente;
                url.Activa = request.Activa;

                await context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "URL actualizada correctamente",
                    data = new
                    {
                        id = url.Id,
                        nombre = url.Nombre,
                        url = url.Url,
                        descripcion = url.Descripcion,
                        tipoFuente = url.TipoFuente,
                        activa = url.Activa
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error actualizando URL {id}");
                return StatusCode(500, new { success = false, message = "Error interno" });
            }
        }

        // MÉTODO PARA NORMALIZAR URLs (MISMO QUE EN SIMULADORSERVICE)
        private string NormalizarUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            var urlLimpia = url.Trim();

            try
            {
                // Si no tiene protocolo, añadir HTTPS
                if (!urlLimpia.StartsWith("http://") && !urlLimpia.StartsWith("https://"))
                {
                    urlLimpia = "https://" + urlLimpia;
                    logger.LogDebug($"🔧 Añadido protocolo HTTPS: {urlLimpia}");
                }

                // Verificar que sea válida
                var uri = new Uri(urlLimpia);
                return uri.ToString();
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Error normalizando URL '{url}': {ex.Message}");
                
                // Fallback con HTTP si HTTPS falla
                try
                {
                    var urlHttp = url.Replace("https://", "").Replace("http://", "");
                    var uriFallback = new Uri("http://" + urlHttp);
                    logger.LogDebug($"🔄 Fallback con HTTP: {uriFallback}");
                    return uriFallback.ToString();
                }
                catch
                {
                    return url; // Devolver original si falla todo
                }
            }
        }

        private string ExtraerNombreDeUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host.Replace("www.", "");
                return char.ToUpper(host[0]) + host.Substring(1);
            }
            catch
            {
                return "Fuente de noticias";
            }
        }

        // ... resto de métodos igual (GET, DELETE, etc.)

        #region Clases Auxiliares
        public class CrearUrlRequest
        {
            public int UsuarioId { get; set; }
            public string Url { get; set; } = string.Empty;
            public string? Nombre { get; set; }
            public int ConfiguracionId { get; set; }
        }

        public class ActualizarUrlRequest
        {
            public string Nombre { get; set; } = string.Empty;
            public string Url { get; set; } = string.Empty;
            public string? Descripcion { get; set; }
            public string TipoFuente { get; set; } = "RSS";
            public bool Activa { get; set; } = true;
        }
        #endregion
    }
}