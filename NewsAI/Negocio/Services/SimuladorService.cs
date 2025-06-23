using NewsAI.Negocio.Interfaces;
using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.API.Controllers;
using NewsAI.Dominio.Entidades;

namespace NewsAI.Negocio.Services
{
    public class SimuladorService : ISimuladorService
    {
        private readonly ILogger<SimuladorService> _logger;
        private readonly IConfiguracionService _configuracionService;
        private readonly INoticiasExtractorService _noticiasExtractorService;
        private readonly IAgenteClasificador _agenteClasificador;
        private readonly IAgenteFiltrador _agenteFiltrador;
        private readonly IAgenteResumidor _agenteResumidor;
        private readonly IEmailService _emailService;

        public SimuladorService(
            ILogger<SimuladorService> logger,
            IConfiguracionService configuracionService,
            INoticiasExtractorService noticiasExtractorService,
            IAgenteClasificador agenteClasificador,
            IAgenteFiltrador agenteFiltrador,
            IAgenteResumidor agenteResumidor,
            IEmailService emailService)
        {
            _logger = logger;
            _configuracionService = configuracionService;
            _noticiasExtractorService = noticiasExtractorService;
            _agenteClasificador = agenteClasificador;
            _agenteFiltrador = agenteFiltrador;
            _agenteResumidor = agenteResumidor;
            _emailService = emailService;
        }

        public async Task<SimuladorResponse> ProcesarNoticiasAsync(SimuladorRequest request)
        {
            _logger.LogInformation($"🎯 SIMULADOR SERVICE - Iniciando procesamiento: {request.UrlNoticias}");

            var tiempoInicio = DateTime.Now;

            try
            {
                // 1. NORMALIZAR URL AUTOMÁTICAMENTE
                var urlNormalizada = NormalizarUrl(request.UrlNoticias);
                _logger.LogInformation($"🔧 URL normalizada: '{request.UrlNoticias}' → '{urlNormalizada}'");

                // 2. OBTENER CONFIGURACIÓN
                _logger.LogInformation("📋 Paso 1: Obteniendo configuración...");
                var configuracion = await _configuracionService.ObtenerPorIdAsync(request.ConfiguracionId);
                if (configuracion == null)
                {
                    throw new Exception("Configuración no encontrada");
                }

                _logger.LogInformation($" Configuración obtenida: {configuracion.Hashtags}");

                // 3. EXTRAER NOTICIAS CON URL NORMALIZADA
                _logger.LogInformation($"📰 Paso 2: Extrayendo noticias de {urlNormalizada}...");
                var noticiasExtraidas = await _noticiasExtractorService.ExtraerNoticiasAsync(
                    urlNormalizada, 
                    Math.Max(request.LimiteNoticias, 30)
                );

                if (!noticiasExtraidas.Any())
                {
                    throw new Exception("No se pudieron extraer noticias de la URL proporcionada");
                }

                _logger.LogInformation($" Extraídas {noticiasExtraidas.Count} noticias");

                // 4. AGENTE CLASIFICADOR
                _logger.LogInformation("🤖 Paso 3: Ejecutando Agente Clasificador...");
                var noticiasClasificadas = await _agenteClasificador.ClasificarNoticiasAsync(noticiasExtraidas);
                _logger.LogInformation($" Clasificadas {noticiasClasificadas.Count} noticias");

                // 5. AGENTE FILTRADOR
                _logger.LogInformation("🔍 Paso 4: Ejecutando Agente Filtrador...");
                var resultadoFiltrado = await _agenteFiltrador.FiltrarNoticiasAsync(noticiasClasificadas, configuracion);
                
                var noticiasRelevantes = resultadoFiltrado.NoticiasRelevantes;
                _logger.LogInformation($" Filtradas: {noticiasRelevantes.Count} relevantes, {resultadoFiltrado.NoticiasDescartadas.Count} descartadas");

                // 6. VERIFICAR SI HAY NOTICIAS RELEVANTES
                if (!noticiasRelevantes.Any())
                {
                    var tiempoSinNoticias = (DateTime.Now - tiempoInicio).TotalSeconds;
                    
                    return new SimuladorResponse
                    {
                        Resumen = "No se encontraron noticias relevantes según tus hashtags de interés. Intenta con diferentes hashtags o una fuente de noticias más amplia.",
                        NoticiasProcesadas = noticiasExtraidas.Count,
                        NoticiasRelevantes = 0,
                        EmailEnviado = false,
                        TiempoProcesamiento = tiempoSinNoticias,
                        RazonFiltrado = resultadoFiltrado.RazonFiltrado,
                        ScoreCoincidencia = resultadoFiltrado.ScoreCoincidencia,
                        DetallesProcessamiento = new DetallesProcesamientoResponse
                        {
                            NoticiasExtraidas = noticiasExtraidas.Count,
                            NoticiasClasificadas = noticiasClasificadas.Count,
                            NoticiasRelevantes = 0,
                            NoticiasDescartadas = resultadoFiltrado.NoticiasDescartadas.Count,
                            ConfiguracionUtilizada = new ConfiguracionUtilizadaResponse
                            {
                                Hashtags = configuracion.Hashtags,
                                Tono = configuracion.Lenguaje,
                                Profundidad = configuracion.GradoDesarrolloResumen,
                                Canal = DeterminarCanalConfiguracion(configuracion)
                            }
                        }
                    };
                }

                // 7. AGENTE RESUMIDOR
                _logger.LogInformation("📝 Paso 5: Ejecutando Agente Resumidor...");
                var resumenGenerado = await _agenteResumidor.GenerarResumenAsync(noticiasRelevantes, configuracion);
                _logger.LogInformation($" Resumen generado: {resumenGenerado.Length} caracteres");

                // 8. ENVÍO DE EMAIL (si está configurado)
                bool emailEnviado = false;
                if (request.EnviarEmail && !configuracion.Audio)
                {
                    _logger.LogInformation("📧 Paso 6: Enviando email...");
                    emailEnviado = await _emailService.EnviarResumenAsync(
                        configuracion.Email ? configuracion.Usuario?.Email : "demo@newsai.com",
                        resumenGenerado,
                        configuracion,
                        noticiasRelevantes.Count);
                    _logger.LogInformation($" Email enviado exitosamente");
                }

                var tiempoTotal = (DateTime.Now - tiempoInicio).TotalSeconds;

                // 9. RESPUESTA EXITOSA
                var response = new SimuladorResponse
                {
                    Resumen = resumenGenerado,
                    NoticiasProcesadas = noticiasExtraidas.Count,
                    NoticiasRelevantes = noticiasRelevantes.Count,
                    EmailEnviado = emailEnviado,
                    TiempoProcesamiento = tiempoTotal,
                    RazonFiltrado = resultadoFiltrado.RazonFiltrado,
                    ScoreCoincidencia = resultadoFiltrado.ScoreCoincidencia,
                    DetallesProcessamiento = new DetallesProcesamientoResponse
                    {
                        NoticiasExtraidas = noticiasExtraidas.Count,
                        NoticiasClasificadas = noticiasClasificadas.Count,
                        NoticiasRelevantes = noticiasRelevantes.Count,
                        NoticiasDescartadas = resultadoFiltrado.NoticiasDescartadas.Count,
                        ConfiguracionUtilizada = new ConfiguracionUtilizadaResponse
                        {
                            Hashtags = configuracion.Hashtags,
                            Tono = configuracion.Lenguaje,
                            Profundidad = configuracion.GradoDesarrolloResumen,
                            Canal = DeterminarCanalConfiguracion(configuracion)
                        }
                    }
                };

                _logger.LogInformation($"🎉 SIMULACIÓN COMPLETADA en {tiempoTotal:F1}s - {noticiasRelevantes.Count} noticias relevantes");
                
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, " Error en procesamiento de simulación");
                throw new Exception($"Error procesando noticias: {ex.Message}");
            }
        }

        // MÉTODO PARA NORMALIZAR URLs
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
                    _logger.LogDebug($"🔧 Añadido protocolo HTTPS: {urlLimpia}");
                }

                // Verificar que sea válida
                var uri = new Uri(urlLimpia);
                return uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"⚠️ Error normalizando URL '{url}': {ex.Message}");
                
                // Fallback con HTTP si HTTPS falla
                try
                {
                    var urlHttp = url.Replace("https://", "").Replace("http://", "");
                    var uriFallback = new Uri("http://" + urlHttp);
                    _logger.LogDebug($"🔄 Fallback con HTTP: {uriFallback}");
                    return uriFallback.ToString();
                }
                catch
                {
                    return url; // Devolver original si falla todo
                }
            }
        }

        public async Task<List<ResumenGenerado>> ObtenerHistorialResumenesAsync(int usuarioId)
        {
            try
            {
                _logger.LogInformation($"Obteniendo historial de resúmenes para usuario {usuarioId}");
                return new List<ResumenGenerado>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo historial para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<ResumenGenerado> ObtenerResumenPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation($"Obteniendo resumen por ID: {id}");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error obteniendo resumen {id}");
                throw;
            }
        }

        private string DeterminarCanalConfiguracion(Configuracion configuracion)
        {
            if (configuracion.Audio) return "Audio";
            if (configuracion.Email) return "Email";
            return "Simulador";
        }
    }
}