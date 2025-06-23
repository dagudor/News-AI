using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.Dominio.Repositorios;
using NewsAI.Dominio.Entidades;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.Negocio.Services;

public class SchedulingService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<SchedulingService> logger;
    private readonly TimeSpan intervaloVerificacion = TimeSpan.FromMinutes(1);

    public SchedulingService(IServiceProvider serviceProvider, ILogger<SchedulingService> logger)
    {
        this.serviceProvider = serviceProvider;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("🕐 SCHEDULER: Servicio de programación iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcesarConfiguracionesPendientes();
                await Task.Delay(intervaloVerificacion, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " Error en el servicio de programación");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        logger.LogInformation("🛑 SCHEDULER: Servicio de programación detenido");
    }

    private async Task ProcesarConfiguracionesPendientes()
    {
        using var scope = serviceProvider.CreateScope();
        var configuracionRepository = scope.ServiceProvider.GetRequiredService<IConfiguracionRepository>();
        var ejecucionRepository = scope.ServiceProvider.GetRequiredService<IEjecucionProgramadaRepository>();

        try
        {
            logger.LogInformation($"🔍 SCHEDULER: Verificando configuraciones - {DateTime.Now:HH:mm:ss}");

            //  USAR MÉTODO EXISTENTE DE TU REPOSITORIO
            var configuracionesPendientes = await configuracionRepository.ObtenerActivasParaEjecucionAsync();

            logger.LogInformation($"📊 SCHEDULER: {configuracionesPendientes.Count} configuraciones pendientes");

            foreach (var configuracion in configuracionesPendientes)
            {
                logger.LogInformation($"⚡ Ejecutando configuración {configuracion.Id} - Usuario {configuracion.UsuarioId}");
                await EjecutarConfiguracionCompleta(configuracion, configuracionRepository, ejecucionRepository);

                // Pausa entre ejecuciones
                await Task.Delay(TimeSpan.FromSeconds(2));
            }

            //  LOG PARA DEBUGGING - SIN MÉTODO QUE NO EXISTE
            if (!configuracionesPendientes.Any())
            {
                logger.LogDebug("⏰ No hay configuraciones pendientes en este momento");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, " SCHEDULER: Error procesando configuraciones pendientes");
        }
    }

    private async Task EjecutarConfiguracionCompleta(
        Configuracion configuracion,
        IConfiguracionRepository configuracionRepository,
        IEjecucionProgramadaRepository ejecucionRepository)
    {
        EjecucionProgramada ejecucion = null;

        try
        {
            // 1. Crear registro de ejecución
            ejecucion = new EjecucionProgramada
            {
                ConfiguracionId = configuracion.Id,
                FechaEjecucion = DateTime.Now,
                Estado = "Ejecutando",
                FechaInicio = DateTime.Now,
                MensajeError = "",
                EmailEnviado = false,
                NoticiasEncontradas = 0,
                NoticiasProcessadas = 0
            };

            ejecucion = await ejecucionRepository.CrearAsync(ejecucion);

            // 2. Obtener servicios necesarios
            using var scope = serviceProvider.CreateScope();
            var noticiasExtractor = scope.ServiceProvider.GetRequiredService<INoticiasExtractorService>();
            var agenteClasificador = scope.ServiceProvider.GetRequiredService<IAgenteClasificador>();
            var agenteFiltrador = scope.ServiceProvider.GetRequiredService<IAgenteFiltrador>();
            var agenteResumidor = scope.ServiceProvider.GetRequiredService<IAgenteResumidor>();
            var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

            // 3. Obtener URLs asociadas a la configuración
            var urlsAsociadas = await ObtenerUrlsDeConfiguracion(configuracion.Id);

            if (!urlsAsociadas.Any())
            {
                throw new InvalidOperationException($"Configuración {configuracion.Id} no tiene URLs asociadas");
            }

            logger.LogInformation($"📡 Procesando {urlsAsociadas.Count} URLs para configuración {configuracion.Id}");

            // 4.  PIPELINE DIRECTO - TIPOS DEFINITIVAMENTE CORRECTOS
            var todasLasNoticiasClasificadas = new List<ClasificacionNoticia>();
            int totalNoticiasExtraidas = 0;

            foreach (var url in urlsAsociadas)
            {
                try
                {
                    logger.LogInformation($"Procesando URL: {url}");

                    // 4a. EXTRAER NOTICIAS
                    var noticiasExtraidas = await noticiasExtractor.ExtraerNoticiasAsync(url, 30);
                    totalNoticiasExtraidas += noticiasExtraidas.Count;

                    if (!noticiasExtraidas.Any())
                    {
                        logger.LogWarning($"⚠️ No se extrajeron noticias de {url}");
                        continue;
                    }

                    // 4b. CLASIFICAR NOTICIAS -  RESULTADO: List<ClasificacionNoticia>
                    var noticiasClasificadas = await agenteClasificador.ClasificarNoticiasAsync(noticiasExtraidas);

                    if (noticiasClasificadas == null || !noticiasClasificadas.Any())
                    {
                        logger.LogWarning($"⚠️ No se clasificaron noticias de {url}");
                        continue;
                    }

                    //  AÑADIR DIRECTAMENTE - YA SON DEL TIPO CORRECTO
                    todasLasNoticiasClasificadas.AddRange(noticiasClasificadas);

                    logger.LogInformation($" URL {url}: {noticiasExtraidas.Count} extraídas, {noticiasClasificadas.Count} clasificadas");
                }
                catch (Exception urlEx)
                {
                    logger.LogError(urlEx, $" Error procesando URL {url}");
                    // Continuar con las demás URLs
                }
            }

            // 5.  FILTRAR TODAS LAS NOTICIAS CLASIFICADAS
            ResultadoFiltrado resultadoFiltrado = null;
            if (todasLasNoticiasClasificadas.Any())
            {
                logger.LogInformation($"🔍 Filtrando {todasLasNoticiasClasificadas.Count} noticias clasificadas");
                resultadoFiltrado = await agenteFiltrador.FiltrarNoticiasAsync(todasLasNoticiasClasificadas, configuracion);
            }

            // 6. GENERAR RESUMEN CONSOLIDADO
            string resumenFinal = "";
            if (resultadoFiltrado?.NoticiasRelevantes?.Any() == true)
            {
                logger.LogInformation($"📝 Generando resumen con {resultadoFiltrado.NoticiasRelevantes.Count} noticias relevantes");
                resumenFinal = await agenteResumidor.GenerarResumenAsync(resultadoFiltrado.NoticiasRelevantes, configuracion);
            }
            else
            {
                resumenFinal = $"No se encontraron noticias relevantes para los hashtags: {configuracion.Hashtags}";
                logger.LogWarning($"⚠️ No hay noticias relevantes para configuración {configuracion.Id}");
            }

            // 7. ENVIAR EMAIL (si está configurado)
            bool emailEnviado = false;
            if (configuracion.Email && !string.IsNullOrEmpty(resumenFinal))
            {
                try
                {
                    await emailService.EnviarResumenAsync(
                        configuracion.Usuario.Email,
                        resumenFinal,
                        configuracion,
                        resultadoFiltrado?.NoticiasRelevantes?.Count ?? 0
                    );
                    emailEnviado = true;
                    logger.LogInformation($"📧 Email enviado exitosamente a {configuracion.Usuario.Email}");
                }
                catch (Exception emailEx)
                {
                    logger.LogError(emailEx, " Error enviando email");
                }
            }

            // Guardado en base de datos
            if (!string.IsNullOrEmpty(resumenFinal))
            {
                try
                {
                    using var scopeResumen = serviceProvider.CreateScope();
                    var simuladorRepository = scopeResumen.ServiceProvider.GetRequiredService<ISimuladorRepository>();

                    var resumenGenerado = new ResumenGenerado
                    {
                        UsuarioId = configuracion.UsuarioId,
                        ConfiguracionId = configuracion.Id,
                        UrlOrigen = string.Join(", ", urlsAsociadas.Take(3)) + (urlsAsociadas.Count > 3 ? "..." : ""),
                        ContenidoResumen = resumenFinal,
                        NoticiasProcesadas = resultadoFiltrado?.NoticiasRelevantes?.Count ?? 0,
                        FechaGeneracion = DateTime.Now,
                        TiempoProcesamiento = (DateTime.Now - ejecucion.FechaInicio.Value).TotalSeconds,
                        EmailEnviado = emailEnviado
                    };

                    await simuladorRepository.GuardarResumenAsync(resumenGenerado);
                    logger.LogInformation(" Resumen guardado en historial de base de datos");
                }
                catch (Exception historialEx)
                {
                    logger.LogWarning($"⚠️ Error guardando resumen en historial: {historialEx.Message}");
                    // No fallar la ejecución por esto
                }

                // 8. ACTUALIZAR EJECUCIÓN COMO COMPLETADA
                ejecucion.Estado = "Completada";
                ejecucion.FechaFin = DateTime.Now;
                ejecucion.NoticiasEncontradas = totalNoticiasExtraidas;
                ejecucion.NoticiasProcessadas = resultadoFiltrado?.NoticiasRelevantes?.Count ?? 0;
                ejecucion.EmailEnviado = emailEnviado;

                await ejecucionRepository.ActualizarAsync(ejecucion);

                // 9.  PROGRAMAR PRÓXIMA EJECUCIÓN USANDO TU MÉTODO
                var proximaEjecucion = configuracion.CalcularProximaEjecucion();
                await configuracionRepository.ActualizarUltimaEjecucionAsync(configuracion.Id, DateTime.Now);
                await configuracionRepository.ActualizarProximaEjecucionAsync(configuracion.Id, proximaEjecucion);

                logger.LogInformation($" Configuración {configuracion.Id} ejecutada exitosamente");
                logger.LogInformation($"📅 Próxima ejecución: {proximaEjecucion:dd/MM/yyyy HH:mm}");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $" Error ejecutando configuración {configuracion.Id}");

            // Marcar ejecución como fallida
            if (ejecucion != null)
            {
                ejecucion.Estado = "Error";
                ejecucion.FechaFin = DateTime.Now;
                ejecucion.MensajeError = ex.Message;

                try
                {
                    await ejecucionRepository.ActualizarAsync(ejecucion);
                }
                catch (Exception updateEx)
                {
                    logger.LogError(updateEx, $" Error actualizando estado de ejecución fallida {ejecucion.Id}");
                }
            }
        }
    }

    private async Task<List<string>> ObtenerUrlsDeConfiguracion(int configuracionId)
    {
        try
        {
            using var scope = serviceProvider.CreateScope();
            var configuracionRepository = scope.ServiceProvider.GetRequiredService<IConfiguracionRepository>();

            //  USAR TU MÉTODO EXISTENTE ObtenerConUrlsAsync
            var configuracion = await configuracionRepository.ObtenerConUrlsAsync(configuracionId);

            if (configuracion?.ConfiguracionUrls != null)
            {
                return configuracion.ConfiguracionUrls
                    .Where(cu => cu.Activa && cu.UrlConfiable?.Activa == true)
                    .Select(cu => cu.UrlConfiable.Url)
                    .Where(url => !string.IsNullOrEmpty(url))
                    .ToList();
            }

            return new List<string>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error obteniendo URLs para configuración {configuracionId}");
            return new List<string>();
        }
    }
}