using System.Text.Json;
using NewsAI.Dominio.Entidades;
using NewsAI.Negocio.Interfaces;
using NewsAI.Negocio.Interfaces.Agentes;

namespace NewsAI.Negocio.Agentes
{
    public class AgenteFiltrador : IAgenteFiltrador
    {
        private readonly ISemanticKernelService _semanticKernelService;
        private readonly ILogger<AgenteFiltrador> _logger;

        public AgenteFiltrador(
            ISemanticKernelService semanticKernelService,
            ILogger<AgenteFiltrador> logger)
        {
            _semanticKernelService = semanticKernelService;
            _logger = logger;
        }

        #region Implementación Principal - ClasificacionNoticia

        // MÉTODO PARA AJUSTAR UMBRAL SI ES NECESARIO
        public void ConfigurarUmbral(double nuevoUmbral)
        {
            _umbralMinimo = Math.Max(0.1, Math.Min(3.0, nuevoUmbral));
            _logger.LogInformation($"🔧 Umbral configurado a: {_umbralMinimo:F2}");
        }

        private double _umbralMinimo = 0.5; // Permisivo pero no estúpido

        // MÉTODO PARA OBTENER ESTADÍSTICAS SIMPLES
        public void LogearEstadisticasHashtags(List<ClasificacionNoticia> noticias, List<string> hashtags)
        {
            _logger.LogInformation("📊 ESTADÍSTICAS POR HASHTAG:");

            foreach (var hashtag in hashtags)
            {
                var hashtagLimpio = hashtag.Replace("#", "").ToLowerInvariant();

                var enTitulos = noticias.Count(n => n.Titulo?.ToLowerInvariant().Contains(hashtagLimpio) == true);
                var enContenidos = noticias.Count(n => n.Contenido?.ToLowerInvariant().Contains(hashtagLimpio) == true);
                var enHashtagsIA = noticias.Count(n => n.HashtagsGenerados?.Any(h => h.ToLowerInvariant().Contains(hashtagLimpio)) == true);

                var total = new HashSet<ClasificacionNoticia>();
                total.UnionWith(noticias.Where(n => n.Titulo?.ToLowerInvariant().Contains(hashtagLimpio) == true));
                total.UnionWith(noticias.Where(n => n.Contenido?.ToLowerInvariant().Contains(hashtagLimpio) == true));
                total.UnionWith(noticias.Where(n => n.HashtagsGenerados?.Any(h => h.ToLowerInvariant().Contains(hashtagLimpio)) == true));

                _logger.LogInformation($"   {hashtag}: {total.Count} noticias (Títulos: {enTitulos}, Contenidos: {enContenidos}, HashtagsIA: {enHashtagsIA})");
            }
        }

       

        /// <summary>
        /// Evalúa si una noticia específica es relevante según hashtags
        /// </summary>
        public async Task<bool> EsRelevantePorHashtagsAsync(ClasificacionNoticia noticia, string hashtagsUsuario)
        {
            _logger.LogDebug($"🔍 Evaluando relevancia individual: {noticia.Titulo}");

            var hashtagsLista = ParsearHashtags(hashtagsUsuario);
            var evaluacion = await EvaluarRelevanciaNoticia(noticia, hashtagsLista);

            _logger.LogDebug($"📊 Resultado evaluación: {evaluacion.EsRelevante} (Score: {evaluacion.Puntuacion})");

            return evaluacion.EsRelevante;
        }

        #endregion

        #region Implementación Legacy - NoticiaExtraida

        /// <summary>
        /// Filtra noticias usando NoticiaExtraida (compatibilidad legacy)
        /// </summary>
        public async Task<ResultadoFiltrado> FiltrarNoticiasAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion)
{
    var resultado = new ResultadoFiltrado();
    
    try
    {
        _logger.LogInformation($"🔍 FILTRADO LIMPIO iniciado con {noticias.Count} noticias");
        _logger.LogInformation($"🏷️ Hashtags usuario: {configuracion.Hashtags}");
        
        var hashtagsUsuario = LimpiarHashtagsUsuario(configuracion.Hashtags);
        
        _logger.LogInformation($"📝 Hashtags procesados: {string.Join(", ", hashtagsUsuario)}");

        // EVALUAR CADA NOTICIA SIN ASUMIR NADA
        foreach (var noticia in noticias)
        {
            var evaluacion = EvaluarNoticiaPura(noticia, hashtagsUsuario);
            
            if (evaluacion.EsRelevante)
            {
                noticia.ScoreFiltrado = evaluacion.ScoreTotal;
                noticia.MotivoRelevancia = evaluacion.MotivoDetallado;
                noticia.HashtagsCoincidentes = evaluacion.CoincidenciasEncontradas;
                
                resultado.NoticiasRelevantes.Add(noticia);
                
                _logger.LogDebug($"✅ RELEVANTE: '{noticia.Titulo.Substring(0, Math.Min(60, noticia.Titulo.Length))}...' " +
                               $"Score: {evaluacion.ScoreTotal:F2} - {evaluacion.MotivoDetallado}");
            }
            else
            {
                resultado.NoticiasDescartadas.Add(noticia);
                
                _logger.LogDebug($"❌ DESCARTADA: '{noticia.Titulo.Substring(0, Math.Min(40, noticia.Titulo.Length))}...' " +
                               $"Score: {evaluacion.ScoreTotal:F2}");
            }
        }

        // ORDENAR POR SCORE (LAS MEJORES PRIMERO)
        resultado.NoticiasRelevantes = resultado.NoticiasRelevantes
            .OrderByDescending(n => n.ScoreFiltrado ?? 0)
            .ToList();

        // ESTADÍSTICAS
        resultado.ScoreCoincidencia = noticias.Any() ? 
            (double)resultado.NoticiasRelevantes.Count / noticias.Count : 0;
        
        resultado.MetodoFiltrado = "Limpio";
        resultado.TotalAnalizadas = noticias.Count;

        _logger.LogInformation($"🎯 FILTRADO LIMPIO COMPLETADO:");
        _logger.LogInformation($"   📊 Total: {noticias.Count}");
        _logger.LogInformation($"   ✅ Relevantes: {resultado.NoticiasRelevantes.Count}");
        _logger.LogInformation($"   ❌ Descartadas: {resultado.NoticiasDescartadas.Count}");
        _logger.LogInformation($"   📈 Porcentaje: {resultado.ScoreCoincidencia:P1}");

        return resultado;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "❌ Error en filtrado limpio");
        throw;
    }
}

        #endregion

        #region Métodos de Evaluación

        private async Task<EvaluacionRelevancia> EvaluarRelevanciaNoticia(
            ClasificacionNoticia noticia,
            List<string> hashtagsUsuario)
        {
            // ESCENARIO 1: La noticia tiene hashtags del clasificador
            if (noticia.HashtagsGenerados != null && noticia.HashtagsGenerados.Any())
            {
                _logger.LogDebug($"🏷️ Noticia con hashtags del clasificador: {string.Join(", ", noticia.HashtagsGenerados)}");

                var evaluacionHashtags = EvaluarHashtagsExistentes(noticia.HashtagsGenerados, hashtagsUsuario);
                if (evaluacionHashtags.EsRelevante)
                {
                    return evaluacionHashtags;
                }
            }

            // ESCENARIO 2: Evaluar por temas detectados
            if (noticia.TemasDetectados != null && noticia.TemasDetectados.Any())
            {
                _logger.LogDebug($"🎯 Evaluando por temas detectados: {string.Join(", ", noticia.TemasDetectados)}");

                var evaluacionTemas = EvaluarPorTemas(noticia.TemasDetectados, hashtagsUsuario);
                if (evaluacionTemas.EsRelevante)
                {
                    return evaluacionTemas;
                }
            }

            // ESCENARIO 3: Generar hashtags con IA si no los tiene
            _logger.LogDebug($"🤖 Generando evaluación IA para: {noticia.Titulo}");

            return await GenerarEvaluacionConIA(noticia, hashtagsUsuario);
        }

        private async Task<EvaluacionRelevancia> EvaluarRelevanciaNoticiaExtraida(
            NoticiaExtraida noticia,
            List<string> hashtagsUsuario)
        {
            // ESCENARIO 1: La noticia tiene hashtags
            if (noticia.Hashtags != null && noticia.Hashtags.Any())
            {
                _logger.LogDebug($"🏷️ Noticia extraída con hashtags: {string.Join(", ", noticia.Hashtags)}");

                return EvaluarHashtagsExistentes(noticia.Hashtags, hashtagsUsuario);
            }

            // ESCENARIO 2: Generar evaluación con IA
            return await GenerarEvaluacionConIAExtraida(noticia, hashtagsUsuario);
        }

        private EvaluacionRelevancia EvaluarHashtagsExistentes(
     List<string> hashtagsNoticia,
     List<string> hashtagsUsuario)
        {
            var hashtagsNoticiaLower = hashtagsNoticia.Select(h => h.ToLower().Trim('#')).ToList();
            var hashtagsUsuarioLower = hashtagsUsuario.Select(h => h.ToLower().Trim('#')).ToList();

            //  1. COINCIDENCIAS EXACTAS
            var coincidenciasExactas = hashtagsNoticiaLower.Intersect(hashtagsUsuarioLower).ToList();

            if (coincidenciasExactas.Any())
            {
                var score = Math.Min(100, coincidenciasExactas.Count * 25);

                return new EvaluacionRelevancia
                {
                    EsRelevante = true,
                    Puntuacion = score,
                    Razon = $"Coincidencias exactas en hashtags: {string.Join(", ", coincidenciasExactas)}"
                };
            }

            //  2. COINCIDENCIAS SEMÁNTICAS (NUEVO)
            var coincidenciasSemanticas = EvaluarCoincidenciasSemanticas(hashtagsNoticiaLower, hashtagsUsuarioLower);

            if (coincidenciasSemanticas.Any())
            {
                var score = Math.Min(95, coincidenciasSemanticas.Count * 20);

                return new EvaluacionRelevancia
                {
                    EsRelevante = true,
                    Puntuacion = score,
                    Razon = $"Coincidencias semánticas: {string.Join(", ", coincidenciasSemanticas)}"
                };
            }

            //  3. COINCIDENCIAS PARCIALES
            var coincidenciasParciales = new List<string>();
            foreach (var hashtagUsuario in hashtagsUsuarioLower)
            {
                var coincidencia = hashtagsNoticiaLower.FirstOrDefault(hn =>
                    hn.Contains(hashtagUsuario) || hashtagUsuario.Contains(hn));

                if (coincidencia != null)
                {
                    coincidenciasParciales.Add($"{hashtagUsuario}≈{coincidencia}");
                }
            }

            if (coincidenciasParciales.Any())
            {
                return new EvaluacionRelevancia
                {
                    EsRelevante = true,
                    Puntuacion = Math.Min(75, coincidenciasParciales.Count * 15),
                    Razon = $"Coincidencias parciales: {string.Join(", ", coincidenciasParciales)}"
                };
            }

            return new EvaluacionRelevancia
            {
                EsRelevante = false,
                Puntuacion = 0,
                Razon = "Sin coincidencias en hashtags"
            };
        }

        //  NUEVO MÉTODO: EVALUACIÓN SEMÁNTICA
        private List<string> EvaluarCoincidenciasSemanticas(List<string> hashtagsNoticia, List<string> hashtagsUsuario)
        {
            var coincidencias = new List<string>();

            //  MAPEO SEMÁNTICO: hashtag_usuario → contextos_relacionados
            var mapaSemantico = new Dictionary<string, string[]>
    {
        // DEPORTES
        { "baloncesto", new[] { "nba", "lakers", "warriors", "celtics", "bulls", "lebron", "curry", "playoffs", "basketball" } },
        { "futbol", new[] { "realmadrid", "barcelona", "laliga", "champions", "messi", "cristiano", "primer", "mundial" } },
        { "tenis", new[] { "atp", "wta", "wimbledon", "rolandgarros", "usopen", "nadal", "djokovic", "federer" } },
        { "f1", new[] { "ferrari", "mercedes", "redbull", "hamilton", "verstappen", "granpremio", "formula1" } },
        
        // TECNOLOGÍA  
        { "tecnologia", new[] { "apple", "iphone", "google", "android", "microsoft", "tesla", "meta", "openai" } },
        { "apple", new[] { "iphone", "ios", "mac", "ipad", "tecnologia", "tech" } },
        { "google", new[] { "android", "chrome", "pixel", "tecnologia", "search" } },
        
        // ENTRETENIMIENTO
        { "cine", new[] { "hollywood", "oscar", "netflix", "disney", "marvel", "entretenimiento" } },
        { "musica", new[] { "spotify", "grammy", "concierto", "album", "entretenimiento" } }
    };

            foreach (var hashtagUsuario in hashtagsUsuario)
            {
                if (mapaSemantico.TryGetValue(hashtagUsuario, out var contextosRelacionados))
                {
                    foreach (var contexto in contextosRelacionados)
                    {
                        var hashtagCoincidente = hashtagsNoticia.FirstOrDefault(hn =>
                            hn.Contains(contexto) || contexto.Contains(hn) ||
                            LevenshteinDistance(hn, contexto) <= 2); // Similaridad por distancia

                        if (hashtagCoincidente != null)
                        {
                            coincidencias.Add($"{hashtagUsuario}⟷{hashtagCoincidente}");
                        }
                    }
                }
            }

            return coincidencias.Distinct().ToList();
        }

        //  MÉTODO AUXILIAR: DISTANCIA DE LEVENSHTEIN (similaridad de cadenas)
        private int LevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a)) return string.IsNullOrEmpty(b) ? 0 : b.Length;
            if (string.IsNullOrEmpty(b)) return a.Length;

            var matrix = new int[a.Length + 1, b.Length + 1];

            for (int i = 0; i <= a.Length; i++) matrix[i, 0] = i;
            for (int j = 0; j <= b.Length; j++) matrix[0, j] = j;

            for (int i = 1; i <= a.Length; i++)
            {
                for (int j = 1; j <= b.Length; j++)
                {
                    var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                    matrix[i, j] = Math.Min(Math.Min(
                        matrix[i - 1, j] + 1,     // deletion
                        matrix[i, j - 1] + 1),    // insertion
                        matrix[i - 1, j - 1] + cost); // substitution
                }
            }

            return matrix[a.Length, b.Length];
        }

        private EvaluacionRelevancia EvaluarPorTemas(
            List<string> temas,
            List<string> hashtagsUsuario)
        {
            var temasLower = temas.Select(t => t.ToLower()).ToList();
            var hashtagsLower = hashtagsUsuario.Select(h => h.ToLower().Trim('#')).ToList();

            var coincidencias = new List<string>();

            foreach (var hashtag in hashtagsLower)
            {
                var tema = temasLower.FirstOrDefault(t =>
                    t.Contains(hashtag) || hashtag.Contains(t));

                if (tema != null)
                {
                    coincidencias.Add($"{hashtag}→{tema}");
                }
            }

            if (coincidencias.Any())
            {
                return new EvaluacionRelevancia
                {
                    EsRelevante = true,
                    Puntuacion = Math.Min(80, coincidencias.Count * 20),
                    Razon = $"Coincidencias en temas: {string.Join(", ", coincidencias)}"
                };
            }

            return new EvaluacionRelevancia
            {
                EsRelevante = false,
                Puntuacion = 0,
                Razon = "Sin coincidencias en temas detectados"
            };
        }

        private async Task<EvaluacionRelevancia> GenerarEvaluacionConIA(
            ClasificacionNoticia noticia,
            List<string> hashtagsUsuario)
        {
            try
            {
                // TODO: Ajustar según tu ISemanticKernelService
                // Opción 1: var kernel = await _semanticKernelService.GetKernelAsync();
                // Opción 2: var kernel = _semanticKernelService.Kernel;
                // Opción 3: Usar directamente _semanticKernelService.InvokePromptAsync(prompt)

                var prompt = $@"
Eres un experto analizador de noticias. Evalúa si esta noticia es relevante para un usuario.

NOTICIA CLASIFICADA:
Título: {noticia.Titulo}
Contenido: {noticia.Contenido?.Substring(0, Math.Min(500, noticia.Contenido?.Length ?? 0))}
Categoría: {noticia.Categoria}
Temas detectados: {string.Join(", ", noticia.TemasDetectados ?? new List<string>())}
Score clasificador: {noticia.ScoreRelevancia:F2}

INTERESES DEL USUARIO: {string.Join(", ", hashtagsUsuario)}

INSTRUCCIONES:
- Evalúa si la noticia coincide con los intereses del usuario
- Considera sinónimos, conceptos relacionados y contexto
- Usa la información de clasificación previa
- Asigna puntuación 0-100

RESPONDE EN JSON:
{{
    ""es_relevante"": true/false,
    ""puntuacion"": 0-100,
    ""razon"": ""Explicación breve""
}}";

                // Ejecutar prompt con tu SemanticKernelService
                var respuestaTexto = await _semanticKernelService.EjecutarPromptAsync(prompt, "gpt-4o-mini");

                _logger.LogDebug($"🤖 Respuesta IA filtrado: {respuestaTexto}");

                var respuestaIA = JsonSerializer.Deserialize<RespuestaEvaluacionIA>(
                    respuestaTexto,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new EvaluacionRelevancia
                {
                    EsRelevante = respuestaIA.Es_Relevante,
                    Puntuacion = respuestaIA.Puntuacion,
                    Razon = respuestaIA.Razon ?? "Evaluación IA sin razón especificada"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error en evaluación IA: {ex.Message}");

                // Fallback: evaluación simple
                var scoreSimple = EvaluarRelevanciaSimple(noticia, hashtagsUsuario);

                return new EvaluacionRelevancia
                {
                    EsRelevante = scoreSimple > 40,
                    Puntuacion = scoreSimple,
                    Razon = "Evaluación texto simple (IA falló)"
                };
            }
        }

        private async Task<EvaluacionRelevancia> GenerarEvaluacionConIAExtraida(
            NoticiaExtraida noticia,
            List<string> hashtagsUsuario)
        {
            try
            {
                var prompt = $@"
Evalúa si esta noticia es relevante para un usuario con intereses específicos.

NOTICIA:
Título: {noticia.Titulo}
Contenido: {noticia.Contenido?.Substring(0, Math.Min(500, noticia.Contenido?.Length ?? 0))}

INTERESES DEL USUARIO: {string.Join(", ", hashtagsUsuario)}

RESPONDE EN JSON:
{{
    ""es_relevante"": true/false,
    ""puntuacion"": 0-100,
    ""razon"": ""Explicación breve""
}}";

                // Ejecutar prompt con tu SemanticKernelService
                var respuestaTexto = await _semanticKernelService.EjecutarPromptAsync(prompt, "gpt-4o-mini");

                var respuestaIA = JsonSerializer.Deserialize<RespuestaEvaluacionIA>(
                    respuestaTexto,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return new EvaluacionRelevancia
                {
                    EsRelevante = respuestaIA.Es_Relevante,
                    Puntuacion = respuestaIA.Puntuacion,
                    Razon = respuestaIA.Razon ?? "Evaluación IA"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($" Error en evaluación IA extraída: {ex.Message}");

                var scoreSimple = EvaluarRelevanciaSimpleExtraida(noticia, hashtagsUsuario);

                return new EvaluacionRelevancia
                {
                    EsRelevante = scoreSimple > 30,
                    Puntuacion = scoreSimple,
                    Razon = "Evaluación texto simple (IA falló)"
                };
            }
        }

        private int EvaluarRelevanciaSimple(ClasificacionNoticia noticia, List<string> hashtagsUsuario)
        {
            var textoCompleto = $"{noticia.Titulo} {noticia.Contenido} {noticia.Categoria} {string.Join(" ", noticia.TemasDetectados ?? new List<string>())}".ToLower();
            int score = 0;

            foreach (var hashtag in hashtagsUsuario)
            {
                var termino = hashtag.ToLower().Trim('#');

                if (noticia.Titulo?.ToLower().Contains(termino) == true) score += 30;
                if (noticia.Contenido?.ToLower().Contains(termino) == true) score += 10;
                if (noticia.Categoria?.ToLower().Contains(termino) == true) score += 25;

                if (noticia.TemasDetectados?.Any(t => t.ToLower().Contains(termino)) == true) score += 20;
            }

            // Bonus por score del clasificador
            if (noticia.ScoreRelevancia > 0.7) score += 5;

            return Math.Min(score, 90);
        }

        private int EvaluarRelevanciaSimpleExtraida(NoticiaExtraida noticia, List<string> hashtagsUsuario)
        {
            var textoCompleto = $"{noticia.Titulo} {noticia.Contenido}".ToLower();
            int score = 0;

            foreach (var hashtag in hashtagsUsuario)
            {
                var termino = hashtag.ToLower().Trim('#');

                if (noticia.Titulo?.ToLower().Contains(termino) == true) score += 30;
                if (noticia.Contenido?.ToLower().Contains(termino) == true) score += 10;
            }

            return Math.Min(score, 60);
        }

        private List<string> ParsearHashtags(string hashtagsTexto)
        {
            if (string.IsNullOrEmpty(hashtagsTexto))
                return new List<string>();

            return hashtagsTexto
                .Split(new[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrEmpty(h))
                .ToList();
        }

        private EvaluacionPura EvaluarNoticiaPura(ClasificacionNoticia noticia, List<string> hashtagsUsuario)
        {
            var eval = new EvaluacionPura();
            var motivos = new List<string>();
            var coincidencias = new List<string>();

            // BUSCAR EN TÍTULO (peso alto)
            var scoreTitulo = BuscarCoincidenciasEnTexto(noticia.Titulo, hashtagsUsuario, coincidencias, "TITULO");
            if (scoreTitulo > 0)
            {
                eval.ScoreTitulo = scoreTitulo * 2.0; // Peso alto para título
                motivos.Add($"Título: {scoreTitulo} coincidencias");
            }

            // BUSCAR EN CONTENIDO (peso medio)
            var scoreContenido = BuscarCoincidenciasEnTexto(noticia.Contenido, hashtagsUsuario, coincidencias, "CONTENIDO");
            if (scoreContenido > 0)
            {
                eval.ScoreContenido = scoreContenido * 1.0; // Peso normal para contenido
                motivos.Add($"Contenido: {scoreContenido} coincidencias");
            }

            // BUSCAR EN HASHTAGS IA (peso alto)
            var scoreHashtagsIA = BuscarCoincidenciasEnLista(noticia.HashtagsGenerados, hashtagsUsuario, coincidencias, "HASHTAGS_IA");
            if (scoreHashtagsIA > 0)
            {
                eval.ScoreHashtagsIA = scoreHashtagsIA * 1.5; // Peso alto para hashtags IA
                motivos.Add($"HashtagsIA: {scoreHashtagsIA} coincidencias");
            }

            // BUSCAR EN CATEGORÍA (peso bajo)
            var scoreCategoria = BuscarCoincidenciasEnTexto(noticia.Categoria, hashtagsUsuario, coincidencias, "CATEGORIA");
            if (scoreCategoria > 0)
            {
                eval.ScoreCategoria = scoreCategoria * 0.5; // Peso bajo para categoría
                motivos.Add($"Categoría: {scoreCategoria} coincidencias");
            }

            // BUSCAR EN TEMAS (peso bajo)
            var scoreTemas = BuscarCoincidenciasEnLista(noticia.TemasDetectados, hashtagsUsuario, coincidencias, "TEMAS");
            if (scoreTemas > 0)
            {
                eval.ScoreTemas = scoreTemas * 0.5; // Peso bajo para temas
                motivos.Add($"Temas: {scoreTemas} coincidencias");
            }

            // CALCULAR SCORE TOTAL
            eval.ScoreTotal = eval.ScoreTitulo + eval.ScoreContenido + eval.ScoreHashtagsIA + eval.ScoreCategoria + eval.ScoreTemas;

            // UMBRAL SIMPLE Y PERMISIVO
            eval.EsRelevante = eval.ScoreTotal >= 0.5; // Si encuentra algo relevante, lo acepta

            // INFORMACIÓN PARA DEBUG
            eval.CoincidenciasEncontradas = coincidencias;
            eval.MotivoDetallado = motivos.Any() ? string.Join(" | ", motivos) : "Sin coincidencias";

            return eval;
        }
        private int BuscarCoincidenciasEnTexto(string? texto, List<string> hashtags, List<string> coincidencias, string contexto)
        {
            if (string.IsNullOrEmpty(texto)) return 0;

            var textoLimpio = texto.ToLowerInvariant()
                                   .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                                   .Replace("ñ", "n");

            int count = 0;

            foreach (var hashtag in hashtags)
            {
                var hashtagLimpio = hashtag.Replace("#", "").ToLowerInvariant()
                                          .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                                          .Replace("ñ", "n");

                if (textoLimpio.Contains(hashtagLimpio))
                {
                    count++;
                    coincidencias.Add($"{hashtag} en {contexto}");
                    _logger.LogDebug($"   ✅ Encontrado '{hashtagLimpio}' en {contexto}");
                }
            }

            return count;
        }

        // BUSCAR COINCIDENCIAS EN LISTA DE STRINGS
        private int BuscarCoincidenciasEnLista(List<string>? lista, List<string> hashtags, List<string> coincidencias, string contexto)
        {
            if (lista?.Any() != true) return 0;

            int count = 0;

            foreach (var item in lista)
            {
                foreach (var hashtag in hashtags)
                {
                    var itemLimpio = item.ToLowerInvariant()
                                        .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                                        .Replace("ñ", "n");

                    var hashtagLimpio = hashtag.Replace("#", "").ToLowerInvariant()
                                              .Replace("á", "a").Replace("é", "e").Replace("í", "i").Replace("ó", "o").Replace("ú", "u")
                                              .Replace("ñ", "n");

                    if (itemLimpio.Contains(hashtagLimpio) || hashtagLimpio.Contains(itemLimpio))
                    {
                        count++;
                        coincidencias.Add($"{hashtag} ≈ {item} en {contexto}");
                        _logger.LogDebug($"   ✅ Encontrado '{hashtagLimpio}' ≈ '{item}' en {contexto}");
                    }
                }
            }

            return count;
        }

        // LIMPIAR HASHTAGS DEL USUARIO SIN ASUMIR NADA
        private List<string> LimpiarHashtagsUsuario(string hashtagsConfig)
        {
            if (string.IsNullOrEmpty(hashtagsConfig)) return new List<string>();

            return hashtagsConfig
                .Split(new char[] { ',', ';', '|' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(h => h.Trim())
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        #endregion
    }

    #region Clases de Soporte

    public class EvaluacionRelevancia
    {
        public bool EsRelevante { get; set; }
        public int Puntuacion { get; set; }
        public string Razon { get; set; }
    }

    public class RespuestaEvaluacionIA
    {
        public bool Es_Relevante { get; set; }
        public int Puntuacion { get; set; }
        public string Razon { get; set; }
    }

    public class EvaluacionPura
    {
        public bool EsRelevante { get; set; }
        public double ScoreTotal { get; set; }
        public double ScoreTitulo { get; set; }
        public double ScoreContenido { get; set; }
        public double ScoreHashtagsIA { get; set; }
        public double ScoreCategoria { get; set; }
        public double ScoreTemas { get; set; }
        public string MotivoDetallado { get; set; } = "";
        public List<string> CoincidenciasEncontradas { get; set; } = new();
    }

    #endregion
}