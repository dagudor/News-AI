using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.Dominio.Entidades;
using System.Text;

namespace NewsAI.Negocio.Services.Agentes
{
   public class AgenteResumidor : IAgenteResumidor
   {
       private readonly Kernel kernel;
       private readonly ILogger<AgenteResumidor> logger;

       public AgenteResumidor(IConfiguration configuration, ILogger<AgenteResumidor> logger)
       {
           this.logger = logger;

           var builder = Kernel.CreateBuilder();
           builder.AddOpenAIChatCompletion(
               "gpt-4", // Modelo más potente para resúmenes de calidad
               configuration["OpenAI:ApiKey"]
           );
           this.kernel = builder.Build();
       }

       public async Task<string> GenerarResumenAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion)
       {
           try
           {
               logger.LogInformation($"📝 Generando resumen de {noticias.Count} noticias con configuración: {configuracion.Lenguaje}, {configuracion.GradoDesarrolloResumen}");

               if (!noticias.Any())
               {
                   return "No hay noticias relevantes para generar un resumen.";
               }

               // SI HAY MUCHAS NOTICIAS, PROCESAR POR LOTES AUTOMÁTICAMENTE
               if (noticias.Count > 8)
               {
                   logger.LogWarning($"⚠️ {noticias.Count} noticias es mucho, procesando por lotes automáticamente");
                   return await ProcesarPorLotes(noticias, configuracion);
               }

               // POCAS NOTICIAS: PROCESAR NORMAL
               var prompt = ConstruirPromptResumen(noticias, configuracion);
               var tokenEstimado = EstimarTokens(prompt);
               logger.LogInformation($"🔢 Tokens estimados: {tokenEstimado:N0}");

               // DOBLE VERIFICACIÓN
               if (tokenEstimado > 40000)
               {
                   logger.LogWarning("⚠️ Prompt muy largo incluso con pocas noticias, forzando lotes");
                   return await ProcesarPorLotes(noticias, configuracion);
               }

               // PROCESAR NORMALMENTE
               var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
               var chatHistory = new ChatHistory();
               chatHistory.AddUserMessage(prompt);

               var response = await chatCompletion.GetChatMessageContentAsync(
                   chatHistory,
                   new OpenAIPromptExecutionSettings()
                   {
                       MaxTokens = ObtenerMaxTokensPorProfundidad(configuracion.GradoDesarrolloResumen),
                       Temperature = 0.7
                   }
               );

               var resumen = response.Content ?? "No se pudo generar el resumen";
               logger.LogInformation($"✅ Resumen generado exitosamente ({resumen.Length} caracteres)");

               return resumen;
           }
           catch (Exception ex)
           {
               logger.LogError(ex, "Error generando resumen");
               throw;
           }
       }

       public async Task<string> GenerarTituloResumenAsync(List<ClasificacionNoticia> noticias, Configuracion configuracion)
       {
           try
           {
               var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

               var temasComunes = noticias.SelectMany(n => n.TemasDetectados)
                   .GroupBy(t => t)
                   .OrderByDescending(g => g.Count())
                   .Take(3)
                   .Select(g => g.Key)
                   .ToList();

               var prompt = $@"
Genera un título atractivo para un resumen de noticias.

NOTICIAS INCLUIDAS: {noticias.Count}
TEMAS PRINCIPALES: {string.Join(", ", temasComunes)}
TONO DESEADO: {configuracion.Lenguaje}

El título debe ser:
- Informativo y específico
- Máximo 60 caracteres
- En tono {configuracion.Lenguaje}

EJEMPLOS:
- ""Resumen Deportivo: Champions League y LaLiga""
- ""Actualidad Política: Elecciones y Reformas""
- ""Tecnología Hoy: IA y Nuevos Gadgets""

Genera solo el título, sin comillas:";

               var chatHistory = new ChatHistory();
               chatHistory.AddUserMessage(prompt);

               var response = await chatCompletion.GetChatMessageContentAsync(
                   chatHistory,
                   new OpenAIPromptExecutionSettings()
                   {
                       MaxTokens = 50,
                       Temperature = 0.8
                   }
               );

               return response.Content ?? $"Resumen de {noticias.Count} noticias";
           }
           catch (Exception ex)
           {
               logger.LogWarning(ex, "Error generando título, usando fallback");
               return $"Resumen de Noticias - {DateTime.Now:dd/MM/yyyy}";
           }
       }

       // PROCESAMIENTO POR LOTES
       private async Task<string> ProcesarPorLotes(List<ClasificacionNoticia> noticias, Configuracion configuracion)
{
    logger.LogInformation($"🔄 Procesando {noticias.Count} noticias en lotes de máximo 3"); // REDUCIDO A 3

    var lotes = CrearLotes(noticias, 3); // LOTES DE SOLO 3 NOTICIAS
    var resumenesParciales = new List<string>();

    for (int i = 0; i < lotes.Count; i++)
    {
        var lote = lotes[i];
        logger.LogInformation($"🔄 Procesando lote {i + 1}/{lotes.Count} ({lote.Count} noticias)");

        try
        {
            var resumenLote = await ProcesarLoteCompleto(lote, configuracion, i + 1, lotes.Count);
            resumenesParciales.Add(resumenLote);

            // Pausa entre lotes
            if (i < lotes.Count - 1)
            {
                await Task.Delay(3000); // AUMENTAR PAUSA A 3 SEGUNDOS
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error en lote {i + 1}");
            resumenesParciales.Add($"[Error procesando lote {i + 1}: {lote.Count} noticias]");
        }
    }

    return await ConsolidarResumenes(resumenesParciales, configuracion);
}

       private List<List<ClasificacionNoticia>> CrearLotes(List<ClasificacionNoticia> noticias, int tamanoLote)
       {
           var lotes = new List<List<ClasificacionNoticia>>();

           for (int i = 0; i < noticias.Count; i += tamanoLote)
           {
               var lote = noticias.Skip(i).Take(tamanoLote).ToList();
               lotes.Add(lote);
           }

           logger.LogInformation($"📦 Creados {lotes.Count} lotes de máximo {tamanoLote} noticias cada uno");

           return lotes;
       }

       private async Task<string> ProcesarLoteCompleto(List<ClasificacionNoticia> lote, Configuracion configuracion, int numeroLote, int totalLotes)
       {
           var prompt = ConstruirPromptLoteCompleto(lote, configuracion, numeroLote, totalLotes);
           
           var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
           var chatHistory = new ChatHistory();
           chatHistory.AddUserMessage(prompt);

           var response = await chatCompletion.GetChatMessageContentAsync(
               chatHistory,
               new OpenAIPromptExecutionSettings()
               {
                   MaxTokens = 800, // Suficiente para un buen resumen parcial
                   Temperature = 0.7
               }
           );

           var resumen = response.Content ?? $"[Error en lote {numeroLote}]";
           logger.LogInformation($"✅ Lote {numeroLote} completado ({resumen.Length} caracteres)");

           return resumen;
       }

       private string ConstruirPromptLoteCompleto(List<ClasificacionNoticia> lote, Configuracion configuracion, int numeroLote, int totalLotes)
{
    var sb = new StringBuilder();

    sb.AppendLine($"Resume estas {lote.Count} noticias (Lote {numeroLote} de {totalLotes}):");
    sb.AppendLine($"Hashtags: {configuracion.Hashtags}");
    sb.AppendLine($"Estilo: {configuracion.Lenguaje}");
    sb.AppendLine();

    sb.AppendLine("INSTRUCCIONES: Resumen DETALLADO de 200-300 palabras");
    sb.AppendLine();

    sb.AppendLine("NOTICIAS:");
    for (int i = 0; i < lote.Count; i++)
    {
        var noticia = lote[i];
        sb.AppendLine($"\n{i + 1}. {noticia.Titulo}");
        
        // TRUNCAR AGRESIVAMENTE EL CONTENIDO
        var contenidoCorto = TruncarTexto(noticia.Contenido, 100); // SOLO 100 CARACTERES
        sb.AppendLine($"   {contenidoCorto}");
    }

    sb.AppendLine("\nGenera resumen detallado:");
    return sb.ToString();
}
       private async Task<string> ConsolidarResumenes(List<string> resumenes, Configuracion configuracion)
       {
           logger.LogInformation($"🔗 Consolidando {resumenes.Count} resúmenes parciales");

           var prompt = ConstruirPromptConsolidacion(resumenes, configuracion);
           
           var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();
           var chatHistory = new ChatHistory();
           chatHistory.AddUserMessage(prompt);

           var response = await chatCompletion.GetChatMessageContentAsync(
               chatHistory,
               new OpenAIPromptExecutionSettings()
               {
                   MaxTokens = ObtenerMaxTokensPorProfundidad(configuracion.GradoDesarrolloResumen),
                   Temperature = 0.7
               }
           );

           return response.Content ?? string.Join("\n\n", resumenes);
       }

       private string ConstruirPromptConsolidacion(List<string> resumenes, Configuracion configuracion)
       {
           var sb = new StringBuilder();

           sb.AppendLine("Eres un editor jefe. Consolida estos resúmenes parciales en un resumen final coherente y completo:");
           sb.AppendLine($"Hashtags de interés: {configuracion.Hashtags}");
           sb.AppendLine($"Estilo: {configuracion.Lenguaje}");
           sb.AppendLine($"Extensión: {configuracion.GradoDesarrolloResumen}");
           sb.AppendLine();

           var extension = configuracion.GradoDesarrolloResumen.ToLower() switch
           {
               "breve" => "400-600 palabras",
               "detallado" => "700-1000 palabras",
               "completo" => "1000-1500 palabras",
               _ => "500-800 palabras"
           };

           sb.AppendLine($"OBJETIVO: Crear un resumen final unificado de {extension}");
           sb.AppendLine("INSTRUCCIONES:");
           sb.AppendLine("- Elimina redundancias entre los resúmenes parciales");
           sb.AppendLine("- Organiza la información de forma coherente y lógica");
           sb.AppendLine("- Conecta los temas cuando sea relevante");
           sb.AppendLine("- Mantén todos los detalles importantes");
           sb.AppendLine("- Estructura en párrafos claros");
           sb.AppendLine();

           for (int i = 0; i < resumenes.Count; i++)
           {
               sb.AppendLine($"--- RESUMEN PARCIAL {i + 1} ---");
               sb.AppendLine(resumenes[i]);
               sb.AppendLine();
           }

           sb.AppendLine("GENERA EL RESUMEN FINAL CONSOLIDADO:");

           return sb.ToString();
       }

       // MÉTODOS AUXILIARES
       private int EstimarTokens(string texto)
       {
           return texto.Length / 4; // Aproximadamente 4 caracteres = 1 token
       }

       private string TruncarTexto(string texto, int maxCaracteres)
       {
           if (string.IsNullOrEmpty(texto) || texto.Length <= maxCaracteres)
               return texto ?? "";

           var truncado = texto.Substring(0, maxCaracteres);
           var ultimoEspacio = truncado.LastIndexOf(' ');
           
           if (ultimoEspacio > maxCaracteres * 0.8)
           {
               truncado = truncado.Substring(0, ultimoEspacio);
           }

           return truncado + "...";
       }

       private string ConstruirPromptResumen(List<ClasificacionNoticia> noticias, Configuracion configuracion)
       {
           var promptBuilder = new StringBuilder();

           promptBuilder.AppendLine("Eres un periodista especializado en crear resúmenes personalizados de noticias.");
           promptBuilder.AppendLine();
           promptBuilder.AppendLine("CONFIGURACIÓN DEL USUARIO:");
           promptBuilder.AppendLine($"- Hashtags de interés: {configuracion.Hashtags}");
           promptBuilder.AppendLine($"- Tono preferido: {configuracion.Lenguaje}");
           promptBuilder.AppendLine($"- Nivel de detalle: {configuracion.GradoDesarrolloResumen}");
           promptBuilder.AppendLine();

           // Instrucciones específicas por configuración
           switch (configuracion.GradoDesarrolloResumen.ToLower())
           {
               case "breve":
                   promptBuilder.AppendLine("INSTRUCCIONES: Crea un resumen SUSTANCIOSO (300-400 palabras mínimo)");
                   break;
               case "detallado":
                   promptBuilder.AppendLine("INSTRUCCIONES: Crea un resumen COMPLETO Y DETALLADO (500-700 palabras mínimo)");
                   break;
               case "completo":
                   promptBuilder.AppendLine("INSTRUCCIONES: Crea un análisis EXTENSO Y PROFUNDO (800-1000 palabras mínimo)");
                   break;
               default:
                   promptBuilder.AppendLine("INSTRUCCIONES: Crea un resumen SUSTANCIOSO (400-500 palabras mínimo)");
                   break;
           }

           promptBuilder.AppendLine("- OBLIGATORIO: Desarrolla cada tema en profundidad");
           promptBuilder.AppendLine("- OBLIGATORIO: Incluye detalles específicos de cada noticia");
           promptBuilder.AppendLine("- OBLIGATORIO: No hagas resúmenes superficiales");

           switch (configuracion.Lenguaje.ToLower())
           {
               case "coloquial":
                   promptBuilder.AppendLine("- Usa un lenguaje CERCANO y fácil de entender");
                   break;
               case "profesional":
                   promptBuilder.AppendLine("- Mantén un tono PROFESIONAL y objetivo");
                   break;
               case "academico":
                   promptBuilder.AppendLine("- Usa un estilo ACADÉMICO con terminología precisa");
                   break;
               default:
                   promptBuilder.AppendLine("- Usa un tono neutral y claro");
                   break;
           }

           promptBuilder.AppendLine("- Enfócate en los hashtags de interés del usuario");
           promptBuilder.AppendLine("- Estructura el contenido de forma clara");
           promptBuilder.AppendLine("- Incluye solo información verificable");
           promptBuilder.AppendLine();

           promptBuilder.AppendLine("NOTICIAS A RESUMIR:");
           promptBuilder.AppendLine("==================");

           for (int i = 0; i < noticias.Count; i++)
           {
               var noticia = noticias[i];
               promptBuilder.AppendLine($"\n--- NOTICIA {i + 1} ---");
               promptBuilder.AppendLine($"Título: {noticia.Titulo}");
               
               if (noticia.TemasDetectados?.Any() == true)
               {
                   promptBuilder.AppendLine($"Temas: {string.Join(", ", noticia.TemasDetectados)}");
               }
               
               promptBuilder.AppendLine($"Contenido: {noticia.Contenido}");
               promptBuilder.AppendLine($"Relevancia: {noticia.ScoreRelevancia:F2}");
           }

           promptBuilder.AppendLine("\n==================");
           promptBuilder.AppendLine("GENERA EL RESUMEN AHORA:");
           promptBuilder.AppendLine("- Desarrolla cada noticia con detalle");
           promptBuilder.AppendLine("- Conecta las noticias entre sí cuando sea relevante");
           promptBuilder.AppendLine("- Proporciona contexto y análisis, no solo hechos");

           return promptBuilder.ToString();
       }

       private int ObtenerMaxTokensPorProfundidad(string profundidad)
       {
           return profundidad.ToLower() switch
           {
               "breve" => 500,      // AUMENTADO
               "detallado" => 1200, // AUMENTADO
               "completo" => 2000,  // AUMENTADO
               _ => 800
           };
       }
   }
}