using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using NewsAI.Negocio.Interfaces;
using NewsAI.Dominio.Entidades;
using System.Text;

namespace NewsAI.Negocio.Services
{
    public class SemanticKernelService : ISemanticKernelService
    {
        private readonly Kernel kernel;
        private readonly Kernel kernelMini; // Para GPT-4o-mini
        private readonly ILogger<SemanticKernelService> logger;

        public SemanticKernelService(IConfiguration configuration, ILogger<SemanticKernelService> logger)
        {
            this.logger = logger;

            // Configurar Semantic Kernel principal (GPT-4)
            var builder = Kernel.CreateBuilder();
            builder.AddOpenAIChatCompletion(
                "gpt-4",
                configuration["OpenAI:ApiKey"]
            );
            this.kernel = builder.Build();

            // Configurar Semantic Kernel para tareas rápidas (GPT-4o-mini)
            var builderMini = Kernel.CreateBuilder();
            builderMini.AddOpenAIChatCompletion(
                "gpt-4o-mini",
                configuration["OpenAI:ApiKey"]
            );
            this.kernelMini = builderMini.Build();
        }

        public Kernel GetKernel()
        {
            return kernel;
        }

        public async Task<string> EjecutarPromptAsync(string prompt)
        {
            return await EjecutarPromptAsync(prompt, "gpt-4o-mini");
        }

        public async Task<string> EjecutarPromptAsync(string prompt, string modelo = "gpt-4o-mini")
        {
            try
            {
                logger.LogDebug($"Ejecutando prompt con modelo: {modelo}");

                // Seleccionar kernel según modelo
                var kernelSeleccionado = modelo.Contains("mini") ? kernelMini : kernel;
                var chatCompletion = kernelSeleccionado.GetRequiredService<IChatCompletionService>();

                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(prompt);

                var response = await chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    new OpenAIPromptExecutionSettings()
                    {
                        MaxTokens = modelo.Contains("mini") ? 1000 : 2000,
                        Temperature = 0.3 // Más determinístico para clasificación/filtrado
                    }
                );

                var resultado = response.Content ?? "No se pudo generar respuesta";
                logger.LogDebug($"Respuesta generada: {resultado.Length} caracteres");

                return resultado;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error ejecutando prompt con modelo {modelo}");
                throw new Exception($"Error en ejecución de prompt: {ex.Message}");
            }
        }

        public async Task<string> GenerarResumenAsync(List<NoticiaExtraida> noticias, Configuracion configuracion)
        {
            try
            {
                logger.LogInformation($"Generando resumen con {noticias.Count} noticias");

                var chatCompletion = kernel.GetRequiredService<IChatCompletionService>();

                // Construir el prompt personalizado
                var prompt = ConstruirPrompt(noticias, configuracion);

                var chatHistory = new ChatHistory();
                chatHistory.AddUserMessage(prompt);

                var response = await chatCompletion.GetChatMessageContentAsync(
                    chatHistory,
                    new OpenAIPromptExecutionSettings()
                    {
                        MaxTokens = ObtenerMaxTokens(configuracion.GradoDesarrolloResumen),
                        Temperature = 0.7
                    }
                );

                var resumen = response.Content ?? "No se pudo generar el resumen";
                logger.LogInformation($"Resumen generado exitosamente ({resumen.Length} caracteres)");

                return resumen;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error generando resumen con Semantic Kernel");
                throw new Exception($"Error en generación de resumen: {ex.Message}");
            }
        }

        private string ConstruirPrompt(List<NoticiaExtraida> noticias, Configuracion configuracion)
        {
            var promptBuilder = new StringBuilder();
            
            // Configuración del asistente
            promptBuilder.AppendLine("Eres un asistente especializado en resumir noticias de forma personalizada.");
            promptBuilder.AppendLine($"CONFIGURACIÓN DEL USUARIO:");
            promptBuilder.AppendLine($"- Tono requerido: {configuracion.Lenguaje}");
            promptBuilder.AppendLine($"- Nivel de detalle: {configuracion.GradoDesarrolloResumen}");
            promptBuilder.AppendLine($"- Hashtags de interés: {configuracion.Hashtags}");
            promptBuilder.AppendLine();

            // Instrucciones específicas según configuración
            promptBuilder.AppendLine("INSTRUCCIONES ESPECÍFICAS:");
            
            // Instrucciones de longitud
            switch (configuracion.GradoDesarrolloResumen.ToLower())
            {
                case "breve":
                    promptBuilder.AppendLine("- Crea un resumen MUY CONCISO de máximo 150 palabras");
                    promptBuilder.AppendLine("- Enfócate solo en los puntos más importantes");
                    break;
                case "detallado":
                    promptBuilder.AppendLine("- Crea un resumen DETALLADO de 300-500 palabras");
                    promptBuilder.AppendLine("- Incluye contexto y análisis de las noticias");
                    break;
                case "completo":
                    promptBuilder.AppendLine("- Crea un análisis COMPLETO de 500-800 palabras");
                    promptBuilder.AppendLine("- Incluye análisis profundo, tendencias y implicaciones");
                    break;
            }

            // Instrucciones de tono
            switch (configuracion.Lenguaje.ToLower())
            {
                case "coloquial":
                    promptBuilder.AppendLine("- Usa un lenguaje CERCANO y fácil de entender");
                    promptBuilder.AppendLine("- Evita tecnicismos excesivos");
                    break;
                case "profesional":
                    promptBuilder.AppendLine("- Mantén un tono PROFESIONAL y objetivo");
                    promptBuilder.AppendLine("- Usa terminología apropiada pero accesible");
                    break;
                case "academico":
                    promptBuilder.AppendLine("- Usa un estilo ACADÉMICO con terminología precisa");
                    promptBuilder.AppendLine("- Incluye análisis crítico y referencias contextuales");
                    break;
            }

            promptBuilder.AppendLine("- Prioriza noticias relacionadas con los hashtags de interés del usuario");
            promptBuilder.AppendLine("- Estructura el resumen de forma clara con párrafos bien definidos");
            promptBuilder.AppendLine("- Incluye solo información verificable y relevante");
            promptBuilder.AppendLine("- Mantén un enfoque objetivo y equilibrado");
            promptBuilder.AppendLine();

            // Añadir las noticias
            promptBuilder.AppendLine("NOTICIAS A RESUMIR:");
            promptBuilder.AppendLine("====================");

            for (int i = 0; i < noticias.Count; i++)
            {
                var noticia = noticias[i];
                promptBuilder.AppendLine($"\n--- NOTICIA {i + 1} ---");
                promptBuilder.AppendLine($"Título: {noticia.Titulo}");
                promptBuilder.AppendLine($"Fuente: {noticia.Fuente}");
                promptBuilder.AppendLine($"Fecha: {noticia.FechaPublicacion:dd/MM/yyyy HH:mm}");
                promptBuilder.AppendLine($"Contenido: {noticia.Contenido}");
                
                if (noticia.Hashtags.Any())
                {
                    promptBuilder.AppendLine($"Hashtags: {string.Join(", ", noticia.Hashtags)}");
                }
                promptBuilder.AppendLine($"URL: {noticia.Url}");
            }

            promptBuilder.AppendLine("\n====================");
            promptBuilder.AppendLine("GENERA EL RESUMEN AHORA siguiendo exactamente las instrucciones de configuración:");

            return promptBuilder.ToString();
        }

        private int ObtenerMaxTokens(string gradoDetalle)
        {
            return gradoDetalle.ToLower() switch
            {
                "breve" => 300,
                "detallado" => 800,
                "completo" => 1200,
                _ => 500
            };
        }
    }
}