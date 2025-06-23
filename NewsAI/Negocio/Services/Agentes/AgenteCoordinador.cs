using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.Negocio.Interfaces;
using NewsAI.Dominio.Repositorios;
using NewsAI.Dominio.Entidades;
using NewsAI.API.Controllers;
using System.Diagnostics;

namespace NewsAI.Negocio.Services.Agentes;

public class AgenteCoordinador : IAgenteCoordinador
{
    private readonly IAgenteClasificador agenteClasificador;
    private readonly IAgenteFiltrador agenteFiltrador;
    private readonly IAgenteResumidor agenteResumidor;
    private readonly INoticiasExtractorService noticiasExtractor;
    private readonly ISimuladorService simuladorService;
    private readonly IEmailService emailService;
    private readonly ISimuladorRepository simuladorRepository;
    private readonly IUrlConfiableRepository urlRepository;
    private readonly IConfiguracionRepository configuracionRepository;
    private readonly IEjecucionProgramadaRepository ejecucionRepository;
    private readonly ILogger<AgenteCoordinador> logger;

    public AgenteCoordinador(
        IAgenteClasificador agenteClasificador,
        IAgenteFiltrador agenteFiltrador,
        IAgenteResumidor agenteResumidor,
        INoticiasExtractorService noticiasExtractor,
        ISimuladorService simuladorService,
        IEmailService emailService,
        ISimuladorRepository simuladorRepository,
        IUrlConfiableRepository urlRepository,
        IConfiguracionRepository configuracionRepository,
        IEjecucionProgramadaRepository ejecucionRepository,
        ILogger<AgenteCoordinador> logger)
    {
        this.agenteClasificador = agenteClasificador;
        this.agenteFiltrador = agenteFiltrador;
        this.agenteResumidor = agenteResumidor;
        this.noticiasExtractor = noticiasExtractor;
        this.simuladorService = simuladorService;
        this.emailService = emailService;
        this.simuladorRepository = simuladorRepository;
        this.urlRepository = urlRepository;
        this.configuracionRepository = configuracionRepository;
        this.ejecucionRepository = ejecucionRepository;
        this.logger = logger;
    }

    public async Task<SimuladorResponse> ProcesarConfiguracionAsync(Configuracion configuracion, List<UrlConfiable> urls)
    {
        var stopwatch = Stopwatch.StartNew();
        var totalNoticiasEncontradas = 0;
        var totalNoticiasRelevantes = 0;

        try
        {
            logger.LogInformation($" COORDINADOR: Iniciando procesamiento para configuración {configuracion.Id}");
            logger.LogInformation($" URLs a procesar: {urls.Count} | Hashtags: {configuracion.Hashtags}");

            // FASE 1: EXTRACCIÓN DE NOTICIAS
            logger.LogInformation(" FASE 1: Extrayendo noticias de todas las URLs...");
            var todasLasNoticias = await ExtraerNoticiasDeUrls(urls);
            totalNoticiasEncontradas = todasLasNoticias.Count;

            if (!todasLasNoticias.Any())
            {
                logger.LogWarning(" No se encontraron noticias en ninguna URL");
                return CrearRespuestaVacia(stopwatch.Elapsed.TotalSeconds);
            }

            logger.LogInformation($" FASE 1 COMPLETADA: {totalNoticiasEncontradas} noticias extraídas");

            // FASE 2: CLASIFICACIÓN CON IA
            logger.LogInformation(" FASE 2: Clasificando noticias con IA...");
            var noticiasClasificadas = await agenteClasificador.ClasificarNoticiasAsync(todasLasNoticias);

            if (!noticiasClasificadas.Any())
            {
                logger.LogWarning(" No se pudieron clasificar las noticias");
                return CrearRespuestaVacia(stopwatch.Elapsed.TotalSeconds);
            }

            logger.LogInformation($" FASE 2 COMPLETADA: {noticiasClasificadas.Count} noticias clasificadas");

            // FASE 3: FILTRADO INTELIGENTE
            logger.LogInformation(" FASE 3: Filtrando noticias relevantes...");
            var resultadoFiltrado = await agenteFiltrador.FiltrarNoticiasAsync(noticiasClasificadas, configuracion);
            totalNoticiasRelevantes = resultadoFiltrado.NoticiasRelevantes.Count;

            if (!resultadoFiltrado.NoticiasRelevantes.Any())
            {
                logger.LogInformation(" No se encontraron noticias relevantes para esta configuración");
                return CrearRespuestaVacia(stopwatch.Elapsed.TotalSeconds, totalNoticiasEncontradas, "No hay noticias relevantes para tus hashtags de interés");
            }

            logger.LogInformation($" FASE 3 COMPLETADA: {totalNoticiasRelevantes} noticias relevantes de {noticiasClasificadas.Count}");

            // FASE 4: GENERACIÓN DE RESUMEN
            logger.LogInformation(" FASE 4: Generando resumen personalizado...");
            var resumen = await agenteResumidor.GenerarResumenAsync(resultadoFiltrado.NoticiasRelevantes, configuracion);

            if (string.IsNullOrEmpty(resumen))
            {
                logger.LogWarning(" No se pudo generar el resumen");
                return CrearRespuestaVacia(stopwatch.Elapsed.TotalSeconds, totalNoticiasEncontradas, "Error generando resumen");
            }

            logger.LogInformation($" FASE 4 COMPLETADA: Resumen generado ({resumen.Length} caracteres)");

            // FASE 5: GUARDAR EN BASE DE DATOS
            logger.LogInformation(" FASE 5: Guardando resultado en base de datos...");
            var resumenGuardado = await GuardarResumenEnBD(configuracion, resumen, totalNoticiasEncontradas, totalNoticiasRelevantes, stopwatch.Elapsed.TotalSeconds);

            // FASE 6: ENVÍO DE EMAIL (si está configurado)
            bool emailEnviado = false;
            if (!configuracion.Audio) // Solo email por ahora
            {
                logger.LogInformation(" FASE 6: Enviando resumen por email...");
                emailEnviado = await emailService.EnviarResumenPorEmailAsync(resumenGuardado, configuracion);

                // Actualizar estado del email en BD
                await simuladorRepository.ActualizarEstadoEmailAsync(resumenGuardado.Id, emailEnviado);

                logger.LogInformation($" Email {(emailEnviado ? "enviado exitosamente" : "falló")}");
            }

            stopwatch.Stop();

            // RESULTADO FINAL
            var respuesta = new SimuladorResponse
            {
                Resumen = resumen,
                NoticiasProcesadas = totalNoticiasRelevantes,
                EmailEnviado = emailEnviado,
                TiempoProcesamiento = stopwatch.Elapsed.TotalSeconds
            };

            logger.LogInformation($" COORDINADOR COMPLETADO EXITOSAMENTE:");
            logger.LogInformation($"    Noticias encontradas: {totalNoticiasEncontradas}");
            logger.LogInformation($"    Noticias relevantes: {totalNoticiasRelevantes}");
            logger.LogInformation($"    Resumen: {resumen.Length} caracteres");
            logger.LogInformation($"    Email: {(emailEnviado ? "" : "")}");
            logger.LogInformation($"   ⏱️ Tiempo total: {stopwatch.Elapsed.TotalSeconds:F2}s");

            return respuesta;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, $" ERROR EN COORDINADOR para configuración {configuracion.Id}");
            throw;
        }
    }

    public async Task<bool> EjecutarConfiguracionProgramadaAsync(int configuracionId)
    {
        EjecucionProgramada ejecucion = null;

        try
        {
            logger.LogInformation($" Iniciando ejecución programada para configuración {configuracionId}");

            // 1. Crear registro de ejecución con TODOS los campos obligatorios
            ejecucion = new EjecucionProgramada
            {
                ConfiguracionId = configuracionId,
                FechaEjecucion = DateTime.UtcNow,
                Estado = "Ejecutando",
                FechaInicio = DateTime.UtcNow,
                EmailEnviado = false,
                NoticiasEncontradas = 0,
                NoticiasProcessadas = 0,
                MensajeError = null
            };

            // 2. Guardar ejecución inicial
            ejecucion = await ejecucionRepository.CrearAsync(ejecucion);

            // 3. Obtener configuración con URLs - MÉTODO CORREGIDO
            var configuracion = await configuracionRepository.ObtenerConUrlsAsync(configuracionId);
            if (configuracion == null)
            {
                await ejecucionRepository.MarcarComoErrorAsync(ejecucion.Id, "Configuración no encontrada");
                return false;
            }

            // 4. Verificar que tenga URLs asociadas
            if (configuracion.ConfiguracionUrls == null || !configuracion.ConfiguracionUrls.Any())
            {
                await ejecucionRepository.MarcarComoErrorAsync(ejecucion.Id, "No hay URLs configuradas");
                return false;
            }

            // 5. Ejecutar pipeline completo para cada URL
            List<ResumenGenerado> resumenes = new List<ResumenGenerado>();
            int totalNoticias = 0;

            foreach (var configUrl in configuracion.ConfiguracionUrls.Where(cu => cu.Activa))
            {
                try
                {
                    var url = configUrl.UrlConfiable.Url;
                    logger.LogInformation($" Procesando URL: {url}");

                    // Simular llamada al pipeline completo
                    var request = new SimuladorRequest
                    {
                        UrlNoticias = url,
                        ConfiguracionId = configuracionId,
                        LimiteNoticias = 10,
                        EnviarEmail = false // Lo enviamos al final
                    };

                    var resultado = await simuladorService.ProcesarNoticiasAsync(request);
                    totalNoticias += resultado.NoticiasProcesadas;

                    logger.LogInformation($" URL procesada: {resultado.NoticiasProcesadas} noticias");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, $" Error procesando URL {configUrl.UrlConfiable.Url}");
                }
            }

            // 6. Enviar email si está configurado
            bool emailEnviado = false;
            if (configuracion.Email && totalNoticias > 0)
            {
                try
                {
                    // Aquí iría la lógica de envío de email
                    logger.LogInformation($" Enviando email a usuario {configuracion.UsuarioId}");
                    emailEnviado = true;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Error enviando email");
                }
            }

            // 7. Actualizar ejecución como completada
            await ejecucionRepository.MarcarComoCompletadaAsync(ejecucion.Id, null);

            // 8. Calcular y actualizar próxima ejecución
            await ActualizarProximaEjecucion(configuracion);

            logger.LogInformation($" Ejecución programada completada - {totalNoticias} noticias procesadas");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $" Error en ejecución programada {configuracionId}");

            // Marcar como error si tenemos el ID de ejecución
            if (ejecucion?.Id > 0)
            {
                await ejecucionRepository.MarcarComoErrorAsync(ejecucion.Id, ex.Message);
            }

            return false;
        }
    }
    /// <summary>
    /// Este metodo calcula y actualiza la siguiente ejecición segun la frecuencia configurada
    /// </summary>
    /// <param name="configuracion"></param>
    /// <returns></returns>
    private async Task ActualizarProximaEjecucion(Configuracion configuracion)
    {
        try
        {
            DateTime? proximaEjecucion = configuracion.Frecuencia.ToLower() switch
            {
                "diaria" => DateTime.UtcNow.Date.AddDays(1).Add(configuracion.HoraEnvio),
                "semanal" => DateTime.UtcNow.Date.AddDays(7).Add(configuracion.HoraEnvio),
                "pausada" => null,
                _ => DateTime.UtcNow.Date.AddDays(1).Add(configuracion.HoraEnvio) // Por defecto diaria
            };

            // Actualizar en la base de datos
            await configuracionRepository.ActualizarProximaEjecucionAsync(configuracion.Id, proximaEjecucion);
            await configuracionRepository.ActualizarUltimaEjecucionAsync(configuracion.Id, DateTime.UtcNow);

            logger.LogInformation($"Próxima ejecución configurada para: {proximaEjecucion}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error actualizando próxima ejecución para configuración {configuracion.Id}");
        }
    }

    private async Task<List<NoticiaExtraida>> ExtraerNoticiasDeUrls(List<UrlConfiable> urls)
    {
        var todasLasNoticias = new List<NoticiaExtraida>();

        // Procesar URLs en paralelo para optimizar tiempo
        var tareasExtraccion = urls.Select(async url =>
        {
            try
            {
                logger.LogDebug($"Extrayendo de: {url.Nombre} ({url.Url})");

                var noticias = await noticiasExtractor.ExtraerNoticiasAsync(url.Url, 10);

                // Actualizar estadísticas de la URL
                await urlRepository.ActualizarEstadisticasExtraccionAsync(url.Id, noticias.Any());

                logger.LogDebug($" {noticias.Count} noticias extraídas de {url.Nombre}");

                return noticias;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, $" Error extrayendo de {url.Nombre}: {ex.Message}");

                // Actualizar estadísticas de error
                await urlRepository.ActualizarEstadisticasExtraccionAsync(url.Id, false);

                return new List<NoticiaExtraida>();
            }
        });

        var resultadosExtraccion = await Task.WhenAll(tareasExtraccion);

        // Combinar todas las noticias
        foreach (var noticias in resultadosExtraccion)
        {
            todasLasNoticias.AddRange(noticias);
        }

        // Eliminar duplicados basados en título
        var noticiasUnicas = todasLasNoticias
            .GroupBy(n => n.Titulo.Trim().ToLower())
            .Select(g => g.First())
            .ToList();

        logger.LogInformation($"Extracción completada: {todasLasNoticias.Count} noticias → {noticiasUnicas.Count} únicas");

        return noticiasUnicas;
    }

    private async Task<ResumenGenerado> GuardarResumenEnBD(
        Configuracion configuracion,
        string resumen,
        int noticiasEncontradas,
        int noticiasRelevantes,
        double tiempoProcessamiento)
    {
        var resumenGenerado = new ResumenGenerado
        {
            UsuarioId = configuracion.UsuarioId,
            ConfiguracionId = configuracion.Id,
            UrlOrigen = "Múltiples URLs configuradas",
            ContenidoResumen = resumen,
            NoticiasProcesadas = noticiasRelevantes,
            FechaGeneracion = DateTime.UtcNow,
            TiempoProcesamiento = tiempoProcessamiento,
            EmailEnviado = false
        };

        return await simuladorRepository.GuardarResumenAsync(resumenGenerado);
    }

    private SimuladorResponse CrearRespuestaVacia(double tiempoProcessamiento, int noticiasEncontradas = 0, string mensaje = "No hay noticias disponibles")
    {
        return new SimuladorResponse
        {
            Resumen = mensaje,
            NoticiasProcesadas = noticiasEncontradas,
            EmailEnviado = false,
            TiempoProcesamiento = tiempoProcessamiento
        };
    }

    public async Task<ResultadoProcesamiento> ProcesarNoticiasCompletaAsync(string url, Configuracion configuracion, int limite = 10)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            logger.LogInformation($" COORDINADOR SIMULADOR: Procesando URL {url} con límite {limite}");
            logger.LogInformation($" Configuración: {configuracion.Hashtags} | Tono: {configuracion.Lenguaje} | Profundidad: {configuracion.GradoDesarrolloResumen}");

            // PASO 1: EXTRACCIÓN
            logger.LogInformation("PASO 1: Extrayendo noticias...");
            var noticiasOriginales = await noticiasExtractor.ExtraerNoticiasAsync(url, limite);

            if (!noticiasOriginales.Any())
            {
                logger.LogWarning("No se pudieron extraer noticias de la URL");
                return new ResultadoProcesamiento
                {
                    NoticiasOriginales = new List<NoticiaExtraida>(),
                    NoticiasRelevantes = new List<ClasificacionNoticia>(),
                    ResumenFinal = "No se encontraron noticias en la URL proporcionada."
                };
            }

            logger.LogInformation($" PASO 1 COMPLETADO: {noticiasOriginales.Count} noticias extraídas");

            // PASO 2: CLASIFICACIÓN
            logger.LogInformation("PASO 2: Clasificando noticias con IA...");
            var noticiasClasificadas = await agenteClasificador.ClasificarNoticiasAsync(noticiasOriginales);

            logger.LogInformation($" PASO 2 COMPLETADO: {noticiasClasificadas.Count} noticias clasificadas");

            // Log de clasificaciones para debug
            foreach (var clasificacion in noticiasClasificadas.Take(3))
            {
                logger.LogDebug($"'{clasificacion.Titulo}' → Categoría: {clasificacion.Categoria}, Hashtags: [{string.Join(", ", clasificacion.HashtagsGenerados)}]");
            }

            // PASO 3: FILTRADO CRÍTICO
            logger.LogInformation($"PASO 3: Filtrando por hashtags del usuario: {configuracion.Hashtags}");
            var resultadoFiltrado = await agenteFiltrador.FiltrarNoticiasAsync(noticiasClasificadas, configuracion);

            logger.LogInformation($"PASO 3 COMPLETADO: {resultadoFiltrado.NoticiasRelevantes.Count} relevantes de {noticiasClasificadas.Count} clasificadas");

            // Log detallado del filtrado para debug
            logger.LogInformation($"RESULTADOS DEL FILTRADO:");
            logger.LogInformation($"    Noticias RELEVANTES: {resultadoFiltrado.NoticiasRelevantes.Count}");
            foreach (var relevante in resultadoFiltrado.NoticiasRelevantes.Take(3))
            {
                logger.LogInformation($"      '{relevante.Titulo.Substring(0, Math.Min(50, relevante.Titulo.Length))}...'");
            }

            logger.LogInformation($"    Noticias DESCARTADAS: {resultadoFiltrado.NoticiasDescartadas.Count}");
            foreach (var descartada in resultadoFiltrado.NoticiasDescartadas.Take(3))
            {
                logger.LogInformation($"       '{descartada.Titulo.Substring(0, Math.Min(50, descartada.Titulo.Length))}...'");
            }

            // PASO 4: GENERACIÓN DE RESUMEN
            string resumenFinal;

            if (resultadoFiltrado.NoticiasRelevantes.Any())
            {
                logger.LogInformation($" PASO 4: Generando resumen con {resultadoFiltrado.NoticiasRelevantes.Count} noticias relevantes");
                resumenFinal = await agenteResumidor.GenerarResumenAsync(resultadoFiltrado.NoticiasRelevantes, configuracion);
            }
            else
            {
                logger.LogWarning(" No hay noticias relevantes para los hashtags especificados");
                resumenFinal = $"A pesar de que las noticias disponibles no se centran en los hashtags de interés del usuario ({configuracion.Hashtags}), se ha realizado un esfuerzo para seleccionar y resumir las noticias más relevantes de las disponibles.\n\n" +
                              "Se recomienda ajustar los hashtags o probar con fuentes de noticias más específicas del tema de interés.";
            }

            logger.LogInformation($" PASO 4 COMPLETADO: Resumen generado ({resumenFinal.Length} caracteres)");

            stopwatch.Stop();

            var resultado = new ResultadoProcesamiento
            {
                NoticiasOriginales = noticiasOriginales,
                NoticiasRelevantes = resultadoFiltrado.NoticiasRelevantes,
                ResumenFinal = resumenFinal,
                TiempoProcessamiento = stopwatch.Elapsed.TotalSeconds
            };

            logger.LogInformation($" COORDINADOR SIMULADOR COMPLETADO:");
            logger.LogInformation($"    {noticiasOriginales.Count} extraídas → {resultadoFiltrado.NoticiasRelevantes.Count} filtradas");
            logger.LogInformation($"    Resumen: {resumenFinal.Length} caracteres");
            logger.LogInformation($"   ⏱️ Tiempo: {stopwatch.Elapsed.TotalSeconds:F2}s");

            return resultado;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, " ERROR en Coordinador Simulador");
            throw;
        }
    }

    public class ResultadoProcesamiento
    {
        public List<NoticiaExtraida> NoticiasOriginales { get; set; } = new();
        public List<ClasificacionNoticia> NoticiasRelevantes { get; set; } = new();
        public string ResumenFinal { get; set; } = string.Empty;
        public double TiempoProcessamiento { get; set; }
    }
}
