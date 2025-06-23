using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimuladorController : ControllerBase
    {
        private readonly ISimuladorService simuladorService;
        private readonly AppDbContext context;
        private readonly ILogger<SimuladorController> logger;

        public SimuladorController(
            ISimuladorService simuladorService,
            AppDbContext context,
            ILogger<SimuladorController> logger)
        {
            this.simuladorService = simuladorService;
            this.context = context;
            this.logger = logger;
        }

        [HttpPost("procesar")]
        public async Task<ActionResult<object>> ProcesarNoticias([FromBody] SimuladorRequest request)
        {
            try
            {
                logger.LogInformation($"SIMULADOR - Iniciando simulación para URL: {request.UrlNoticias}");

                var resultado = await simuladorService.ProcesarNoticiasAsync(request);

                return Ok(new
                {
                    success = true,
                    message = "Simulación completada correctamente",
                    resumen = resultado.Resumen,
                    noticiasProcesadas = resultado.NoticiasProcesadas,
                    noticiasRelevantes = resultado.NoticiasRelevantes ?? resultado.NoticiasProcesadas,
                    emailEnviado = resultado.EmailEnviado,
                    tiempoProcesamiento = resultado.TiempoProcesamiento,
                    razonFiltrado = resultado.RazonFiltrado,
                    scoreCoincidencia = resultado.ScoreCoincidencia ?? 0.0,
                    detallesProcessamiento = resultado.DetallesProcessamiento != null ? new
                    {
                        noticiasExtraidas = resultado.DetallesProcessamiento.NoticiasExtraidas,
                        noticiasClasificadas = resultado.DetallesProcessamiento.NoticiasClasificadas,
                        noticiasRelevantes = resultado.DetallesProcessamiento.NoticiasRelevantes,
                        noticiasDescartadas = resultado.DetallesProcessamiento.NoticiasDescartadas,
                        configuracionUtilizada = resultado.DetallesProcessamiento.ConfiguracionUtilizada != null ? new
                        {
                            hashtags = resultado.DetallesProcessamiento.ConfiguracionUtilizada.Hashtags,
                            tono = resultado.DetallesProcessamiento.ConfiguracionUtilizada.Tono,
                            profundidad = resultado.DetallesProcessamiento.ConfiguracionUtilizada.Profundidad,
                            canal = resultado.DetallesProcessamiento.ConfiguracionUtilizada.Canal
                        } : null
                    } : null
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en simulación");
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Error en simulación: {ex.Message}"
                });
            }
        }

        [HttpGet("historial/{usuarioId}")]
        public async Task<ActionResult<object>> ObtenerHistorialResumenesUsuario(int usuarioId, [FromQuery] int limite = 30)
        {
            try
            {
                logger.LogInformation($"HISTORIAL - Usuario {usuarioId}, límite {limite}");

                // Obtener datos básicos SIN Include() para evitar problemas de relaciones
                var resumenes = await context.Resumen
                    .Where(r => r.UsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaGeneracion)
                    .Take(limite)
                    .ToListAsync();

                if (!resumenes.Any())
                {
                    logger.LogInformation($"No se encontraron resúmenes para usuario {usuarioId}");
                    return Ok(new
                    {
                        success = true,
                        message = "No hay resúmenes disponibles",
                        data = new List<object>(),
                        total = 0,
                        mostrando = 0
                    });
                }

                var configuracionIds = resumenes.Select(r => r.ConfiguracionId).Distinct().ToList();
                var configuraciones = await context.Configuraciones
                    .Where(c => configuracionIds.Contains(c.Id))
                    .ToDictionaryAsync(c => c.Id, c => c);

                var resumenesRecientes = resumenes
                    .OrderByDescending(r => r.FechaGeneracion)
                    .Take(limite)
                    .Select(resumen =>
                    {
                        var config = configuraciones.GetValueOrDefault(resumen.ConfiguracionId);

                        return new
                        {
                            id = resumen.Id,
                            titulo = GenerarTituloResumen(resumen.ContenidoResumen),
                            extracto = GenerarExtracto(resumen.ContenidoResumen),
                            contenidoCompleto = resumen.ContenidoResumen,
                            contenidoResumen = resumen.ContenidoResumen ?? "",
                            urlOrigen = resumen.UrlOrigen ?? "",
                            fechaGeneracion = resumen.FechaGeneracion,
                            fechaFormateada = resumen.FechaGeneracion.ToString("dd/MM/yyyy HH:mm"),
                            fechaRelativa = CalcularTiempoRelativo(resumen.FechaGeneracion),
                            noticiasProcessadas = resumen.NoticiasProcesadas,
                            tiempoProcessamiento = resumen.TiempoProcesamiento,
                            emailEnviado = resumen.EmailEnviado,

                            configuracion = new
                            {
                                id = resumen.ConfiguracionId,
                                hashtags = config?.Hashtags ?? "Sin hashtags específicos",
                                tono = config?.Lenguaje ?? "Estándar",
                                profundidad = config?.GradoDesarrolloResumen ?? "Media",
                                accion = config?.Audio == true ? "audio" : "email"
                            },

                            // Estadísticas del resumen
                            estadisticas = new
                            {
                                palabrasResumen = ContarPalabras(resumen.ContenidoResumen),
                                caracteresResumen = resumen.ContenidoResumen?.Length ?? 0
                            }
                        };
                    })
                    .ToList();

                logger.LogInformation($"Devolviendo {resumenesRecientes.Count} resúmenes con configuraciones reales");

                return Ok(new
                {
                    success = true,
                    message = "Historial obtenido correctamente",
                    data = resumenesRecientes,
                    total = resumenes.Count,
                    mostrando = resumenesRecientes.Count
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo historial para usuario {usuarioId}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        [HttpGet("resumen/{id}")]
        public async Task<ActionResult<object>> ObtenerDetalleResumen(int id)
        {
            try
            {
                logger.LogInformation($"Obteniendo detalle del resumen {id}");

                var resumen = await context.Resumen
                    .FirstOrDefaultAsync(r => r.Id == id);

                if (resumen == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Resumen no encontrado"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "Detalle obtenido correctamente",
                    data = new
                    {
                        id = resumen.Id,
                        contenidoCompleto = resumen.ContenidoResumen,
                        urlOrigen = resumen.UrlOrigen,
                        fechaGeneracion = resumen.FechaGeneracion,
                        fechaFormateada = resumen.FechaGeneracion.ToString("dd/MM/yyyy HH:mm"),
                        noticiasProcesadas = resumen.NoticiasProcesadas,
                        tiempoProcessamiento = resumen.TiempoProcesamiento,
                        emailEnviado = resumen.EmailEnviado,
                        usuario = new
                        {
                            id = resumen.UsuarioId,
                        },
                        configuracion = new
                        {
                            id = resumen.ConfiguracionId,
                            hashtags = "Por determinar",
                            tono = "Por determinar",
                            profundidad = "Por determinar",
                            accion = "email"
                        },
                        estadisticas = new
                        {
                            palabrasResumen = ContarPalabras(resumen.ContenidoResumen),
                            caracteresResumen = resumen.ContenidoResumen?.Length ?? 0,
                            tiempoLectura = CalcularTiempoLectura(resumen.ContenidoResumen)
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo detalle del resumen {id}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno del servidor"
                });
            }
        }

        #region Funciones privadas
        private string GenerarTituloResumen(string contenidoResumen)
        {
            if (string.IsNullOrEmpty(contenidoResumen))
                return "Resumen de Noticias";

            try
            {
                var palabras = contenidoResumen.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var titulo = string.Join(" ", palabras.Take(8));
                return titulo.Length > 50 ? titulo.Substring(0, 47) + "..." : titulo;
            }
            catch
            {
                return "Resumen de Noticias";
            }
        }

        private string GenerarExtracto(string contenidoResumen)
        {
            if (string.IsNullOrEmpty(contenidoResumen))
                return "Sin contenido disponible";

            try
            {
                var oraciones = contenidoResumen.Split('.', StringSplitOptions.RemoveEmptyEntries);

                if (oraciones.Length >= 2)
                {
                    var extracto = string.Join(". ", oraciones.Take(2)) + ".";
                    return extracto.Length > 150 ? extracto.Substring(0, 147) + "..." : extracto;
                }

                return contenidoResumen.Length > 150 ? contenidoResumen.Substring(0, 147) + "..." : contenidoResumen;
            }
            catch
            {
                return "Sin contenido disponible";
            }
        }
        /// <summary>
        /// Metodo que calcula el tiempo desde la fecha de generación del resumen.
        /// </summary>
        /// <param name="fechaGeneracion"></param>
        /// <returns></returns>
        private string CalcularTiempoRelativo(DateTime fechaGeneracion)
        {
            try
            {
                var ahora = DateTime.UtcNow;
                var diferencia = ahora - fechaGeneracion;

                if (diferencia.TotalMinutes < 1)
                    return "Hace un momento";
                else if (diferencia.TotalMinutes < 60)
                    return $"Hace {(int)diferencia.TotalMinutes} minutos";
                else if (diferencia.TotalHours < 24)
                    return $"Hace {(int)diferencia.TotalHours} horas";
                else if (diferencia.TotalDays < 7)
                    return $"Hace {(int)diferencia.TotalDays} días";
                else
                    return fechaGeneracion.ToString("dd/MM/yyyy");
            }
            catch
            {
                return "Fecha no disponible";
            }
        }

        /// <summary>
        /// Metodo que cuenta las palabras en un texto.
        /// </summary>
        /// <param name="texto"></param>
        /// <returns></returns>
        private int ContarPalabras(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return 0;

            try
            {
                return texto.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            }
            catch
            {
                return 0;
            }
        }

        private string CalcularTiempoLectura(string texto)
        {
            try
            {
                var palabras = ContarPalabras(texto);
                var minutos = Math.Ceiling(palabras / 200.0); // Promedio 200 palabras por minuto
                return minutos == 1 ? "1 minuto" : $"{minutos} minutos";
            }
            catch
            {
                return "Tiempo no disponible";
            }
        }
        #endregion
    }

    #region Clases Auxiliares
    public class SimuladorRequest
    {
        public string UrlNoticias { get; set; }
        public int ConfiguracionId { get; set; }
        public int LimiteNoticias { get; set; } = 10;
        public bool EnviarEmail { get; set; } = false;
    }
    public class SimuladorResponse
    {
        public string Resumen { get; set; }
        public int NoticiasProcesadas { get; set; }
        public int? NoticiasRelevantes { get; set; }
        public bool EmailEnviado { get; set; }
        public double TiempoProcesamiento { get; set; }
        public string RazonFiltrado { get; set; }
        public double? ScoreCoincidencia { get; set; }
        public DetallesProcesamientoResponse DetallesProcessamiento { get; set; }
    }
    public class DetallesProcesamientoResponse
    {
        public int NoticiasExtraidas { get; set; }
        public int NoticiasClasificadas { get; set; }
        public int NoticiasRelevantes { get; set; }
        public int NoticiasDescartadas { get; set; }
        public ConfiguracionUtilizadaResponse ConfiguracionUtilizada { get; set; }
    }
    public class ConfiguracionUtilizadaResponse
    {
        public string Hashtags { get; set; }
        public string Tono { get; set; }
        public string Profundidad { get; set; }
        public string Canal { get; set; }
    }
    #endregion
}