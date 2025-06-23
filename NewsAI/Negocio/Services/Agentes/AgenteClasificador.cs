using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.Dominio.Entidades;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;
using System.Text.RegularExpressions;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.Negocio.Services.Agentes
{
    public class AgenteClasificador : IAgenteClasificador
    {
        private readonly Kernel kernel;
        private readonly ILogger<AgenteClasificador> logger;
        private readonly IConocimientoBaseService conocimientoService;

        public AgenteClasificador(
            IConfiguration configuration, 
            ILogger<AgenteClasificador> logger, 
            IConocimientoBaseService conocimientoService)
        {
            this.logger = logger;
            this.conocimientoService = conocimientoService;
            
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                "gpt-4o-mini",
                configuration["OpenAI:ApiKey"]
            );
            this.kernel = builder.Build();
        }

        public async Task<List<ClasificacionNoticia>> ClasificarNoticiasAsync(List<NoticiaExtraida> noticias)
        {
            try
            {
                logger.LogInformation($"🔍 Iniciando clasificación HÍBRIDA de {noticias.Count} noticias");

                var noticiasClasificadas = new List<ClasificacionNoticia>();

                foreach (var noticia in noticias)
                {
                    try
                    {
                        var clasificacion = await ClasificarNoticiaAsync(noticia);
                        noticiasClasificadas.Add(clasificacion);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, $"Error clasificando noticia: {noticia.Titulo}");
                        noticiasClasificadas.Add(CrearClasificacionFallback(noticia));
                    }
                }

                logger.LogInformation($" Clasificación completada: {noticiasClasificadas.Count}/{noticias.Count}");
                return noticiasClasificadas;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en clasificación masiva");
                throw;
            }
        }

        public async Task<ClasificacionNoticia> ClasificarNoticiaAsync(NoticiaExtraida noticia)
        {
            try
            {
                logger.LogDebug($"🔍 Clasificando: {noticia.Titulo}");

                // PASO 1: Análisis local con JsonConocimiento
                var analisisLocal = await AnalizarConConocimientoLocalAsync(noticia);
                
                // PASO 2: Decidir si necesitamos IA o no
                if (analisisLocal.ScoreConfianza > 0.6)
                {
                    // Confiamos en el análisis local
                    logger.LogDebug($" Score local alto ({analisisLocal.ScoreConfianza:F2}), usando conocimiento");
                    return ConvertirAClasificacion(noticia, analisisLocal, "ConocimientoLocal");
                }
                else
                {
                    // Complementar con IA
                    logger.LogDebug($"🤖 Score local bajo ({analisisLocal.ScoreConfianza:F2}), complementando con IA");
                    return await ComplementarConIAAsync(noticia, analisisLocal);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error clasificando: {noticia.Titulo}");
                return CrearClasificacionFallback(noticia);
            }
        }

        // ===== MÉTODO PRINCIPAL: ANÁLISIS CON CONOCIMIENTO =====
        private async Task<AnalisisLocal> AnalizarConConocimientoLocalAsync(NoticiaExtraida noticia)
        {
            try
            {
                // Usar directamente el servicio de conocimiento
                var resultado = await conocimientoService.ClasificarTextoAsync(noticia.Contenido, noticia.Titulo);

                return new AnalisisLocal
                {
                    ContextoPrincipal = resultado.ContextoPrincipal,
                    Categoria = resultado.Categoria,
                    Hashtags = resultado.HashtagsDetectados ?? new List<string>(),
                    PalabrasClave = resultado.PalabrasClaveEncontradas ?? new List<string>(),
                    ScoreConfianza = resultado.ScoreRelevancia
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error en análisis local, usando fallback básico");
                
                // Fallback: análisis básico de texto
                return AnalizarTextoBasico(noticia);
            }
        }

        // ===== ANÁLISIS BÁSICO (FALLBACK SIMPLE) =====
        private AnalisisLocal AnalizarTextoBasico(NoticiaExtraida noticia)
        {
            var texto = $"{noticia.Titulo} {noticia.Contenido}".ToLower();
            var hashtags = new List<string>();
            var palabrasClave = new List<string>();
            var categoria = "general";

            // Extracción simple de palabras importantes (sin hardcodear temas específicos)
            var palabrasImportantes = Regex.Matches(texto, @"\b[a-záéíóúñ]{4,}\b", RegexOptions.IgnoreCase)
                .Cast<Match>()
                .Select(m => m.Value.ToLower())
                .Where(p => !EsPalabraComun(p))
                .GroupBy(p => p)
                .OrderByDescending(g => g.Count())
                .Take(8)
                .Select(g => g.Key)
                .ToList();

            foreach (var palabra in palabrasImportantes)
            {
                palabrasClave.Add(palabra);
                hashtags.Add($"#{palabra}");
            }

            return new AnalisisLocal
            {
                ContextoPrincipal = "general",
                Categoria = categoria,
                Hashtags = hashtags,
                PalabrasClave = palabrasClave,
                ScoreConfianza = 0.3 // Score bajo para que se complemente con IA
            };
        }

        // ===== COMPLEMENTAR CON IA (VERSIÓN SIMPLE) =====
        private async Task<ClasificacionNoticia> ComplementarConIAAsync(NoticiaExtraida noticia, AnalisisLocal analisisLocal)
        {
            try
            {
                // Obtener contextos disponibles del JsonConocimiento
                var baseConocimiento = await conocimientoService.CargarBaseConocimientoAsync();
                var contextosDisponibles = string.Join(", ", baseConocimiento.Contextos.Keys);

                var prompt = $@"
Analiza esta noticia y clasifícala usando los CONTEXTOS DISPONIBLES:

CONTEXTOS: {contextosDisponibles}

NOTICIA:
Título: {noticia.Titulo}
Contenido: {noticia.Contenido.Substring(0, Math.Min(800, noticia.Contenido.Length))}

ANÁLISIS PREVIO (puede estar incorrecto):
- Contexto: {analisisLocal.ContextoPrincipal}
- Categoría: {analisisLocal.Categoria}

INSTRUCCIONES:
1. Identifica el CONTEXTO correcto de la lista disponible
2. Genera 3-5 hashtags relevantes
3. Extrae 3-5 temas específicos
4. Asigna categoría y score (0.0-1.0)

Respuesta JSON:
{{
    ""contexto"": ""contexto_de_la_lista"",
    ""categoria"": ""categoria"",
    ""hashtags"": [""#tag1"", ""#tag2"", ""#tag3""],
    ""temas"": [""tema1"", ""tema2"", ""tema3""],
    ""score"": 0.8
}}";

                var respuestaIA = await EjecutarPromptAsync(prompt);
                return ParsearRespuestaIA(respuestaIA, noticia, analisisLocal);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error complementando con IA, usando análisis local");
                return ConvertirAClasificacion(noticia, analisisLocal, "LocalConError");
            }
        }

        // ===== MÉTODOS AUXILIARES SIMPLES =====
        private async Task<string> EjecutarPromptAsync(string prompt)
        {
            var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
            var chatHistory = new ChatHistory();
            chatHistory.AddUserMessage(prompt);

            var response = await chatCompletion.GetChatMessageContentAsync(
                chatHistory,
                new OpenAIPromptExecutionSettings()
                {
                    MaxTokens = 400,
                    Temperature = 0.1
                }
            );

            return response.Content ?? "{}";
        }

        private ClasificacionNoticia ParsearRespuestaIA(string respuestaJson, NoticiaExtraida noticia, AnalisisLocal analisisLocal)
        {
            try
            {
                var jsonLimpio = LimpiarJson(respuestaJson);
                using var document = JsonDocument.Parse(jsonLimpio);
                var root = document.RootElement;

                var contexto = root.TryGetProperty("contexto", out var contextoEl) 
                    ? contextoEl.GetString() ?? analisisLocal.ContextoPrincipal 
                    : analisisLocal.ContextoPrincipal;

                var categoria = root.TryGetProperty("categoria", out var categoriaEl) 
                    ? categoriaEl.GetString() ?? analisisLocal.Categoria 
                    : analisisLocal.Categoria;

                var hashtags = ExtraerArray(root, "hashtags").Any() 
                    ? ExtraerArray(root, "hashtags") 
                    : analisisLocal.Hashtags;

                var temas = ExtraerArray(root, "temas").Any() 
                    ? ExtraerArray(root, "temas") 
                    : analisisLocal.PalabrasClave;

                var score = root.TryGetProperty("score", out var scoreEl) 
                    ? ParsearScore(scoreEl) 
                    : Math.Max(analisisLocal.ScoreConfianza, 0.7);

                return new ClasificacionNoticia
                {
                    Titulo = noticia.Titulo,
                    Contenido = noticia.Contenido,
                    UrlOriginal = noticia.Url,
                    TemasDetectados = temas.Take(8).ToList(),
                    HashtagsGenerados = hashtags.Take(8).ToList(),
                    Categoria = categoria.ToLower(),
                    ScoreRelevancia = score,
                    ContextoDetectado = contexto,
                    MetodoClasificacion = "HibridoIA",
                    FechaClasificacion = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Error parseando IA, usando análisis local");
                return ConvertirAClasificacion(noticia, analisisLocal, "LocalPorErrorIA");
            }
        }

        private ClasificacionNoticia ConvertirAClasificacion(NoticiaExtraida noticia, AnalisisLocal analisis, string metodo)
        {
            return new ClasificacionNoticia
            {
                Titulo = noticia.Titulo,
                Contenido = noticia.Contenido,
                UrlOriginal = noticia.Url,
                TemasDetectados = analisis.PalabrasClave.Take(8).ToList(),
                HashtagsGenerados = analisis.Hashtags.Take(8).ToList(),
                Categoria = analisis.Categoria,
                ScoreRelevancia = analisis.ScoreConfianza,
                ContextoDetectado = analisis.ContextoPrincipal,
                MetodoClasificacion = metodo,
                FechaClasificacion = DateTime.UtcNow
            };
        }

        private ClasificacionNoticia CrearClasificacionFallback(NoticiaExtraida noticia)
        {
            logger.LogInformation($"Fallback para: {noticia.Titulo}");

            return new ClasificacionNoticia
            {
                Titulo = noticia.Titulo,
                Contenido = noticia.Contenido,
                UrlOriginal = noticia.Url,
                TemasDetectados = new List<string> { "general" },
                HashtagsGenerados = new List<string> { "#general" },
                Categoria = "general",
                ScoreRelevancia = 0.5,
                ContextoDetectado = "general",
                MetodoClasificacion = "Fallback",
                FechaClasificacion = DateTime.UtcNow
            };
        }

        // ===== UTILIDADES BÁSICAS =====
        private List<string> ExtraerArray(JsonElement root, string propiedad)
        {
            try
            {
                if (root.TryGetProperty(propiedad, out var elemento) && elemento.ValueKind == JsonValueKind.Array)
                {
                    return elemento.EnumerateArray()
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrEmpty(s))
                        .ToList();
                }
            }
            catch { }
            return new List<string>();
        }

        private double ParsearScore(JsonElement scoreElement)
        {
            try
            {
                if (scoreElement.ValueKind == JsonValueKind.Number)
                    return scoreElement.GetDouble();
                if (double.TryParse(scoreElement.GetString(), out var score))
                    return score;
            }
            catch { }
            return 0.7;
        }

        private string LimpiarJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return "{}";

            try
            {
                json = Regex.Replace(json, @"```json\s*", "", RegexOptions.IgnoreCase);
                json = Regex.Replace(json, @"```\s*$", "", RegexOptions.IgnoreCase);
                
                var match = Regex.Match(json, @"\{.*\}", RegexOptions.Singleline);
                return match.Success ? match.Value.Trim() : json.Trim();
            }
            catch
            {
                return json.Trim();
            }
        }

        private bool EsPalabraComun(string palabra)
        {
            // Lista básica de palabras comunes españolas
            var palabrasComunes = new HashSet<string>
            {
                "el", "la", "de", "que", "y", "a", "en", "un", "es", "se", "no", "te", "lo", "le", "da", "su", "por", "son", "con", "para", "al", "del", "los", "las", "una", "sobre", "todo", "pero", "más", "muy", "como", "ya", "otros", "hasta", "hacer", "este", "sido", "tiene", "cada", "está", "este", "bien", "puede", "entre", "sin", "desde", "cuando", "donde", "será", "algo", "porque", "también", "nos", "así", "mucho", "año", "años", "día", "días", "vez", "veces", "tiempo", "tanto", "forma", "manera", "caso", "casos", "parte", "lugar", "lugares", "trabajo", "vida", "mundo", "país", "países", "hombre", "mujer", "persona", "personas", "casa", "ciudad", "agua", "hora", "horas", "momento", "momentos", "tipo", "tipos", "cosa", "cosas", "través", "después", "antes", "durante", "dentro", "fuera", "cerca", "lejos", "arriba", "abajo", "aquí", "allí", "ahora", "luego", "nunca", "siempre", "menos", "mejor", "peor", "mayor", "menor", "mismo", "misma", "otros", "otras", "nuevo", "nueva", "gran", "grande", "pequeño", "pequeña", "último", "última", "primer", "primera", "segundo", "segunda", "tercero", "tercera"
            };

            return palabrasComunes.Contains(palabra);
        }
    }

    // ===== CLASE AUXILIAR SIMPLE =====
    public class AnalisisLocal
    {
        public string ContextoPrincipal { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public List<string> Hashtags { get; set; } = new();
        public List<string> PalabrasClave { get; set; } = new();
        public double ScoreConfianza { get; set; }
    }
}