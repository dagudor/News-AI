using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace NewsAI.Skils
{
    public class SummarizePlugin
    {

        [KernelFunction, Description("Resume un texto en tres lineas manteniendo datos clave")]
        public async Task<string> Summarize(Kernel kernel, [Description("Texto completo a resumir")] string textoAResumir)
        {
            var prompt = @"Resume el siguiente  texto en tres lineas
                            - Mateniendo nombres fechas y numeros importantes
                            - Usa lenguage claro y conciso
                            - Evita redundancias 
                            Texto: {{$textoAResumir}}";
            return await kernel
                .InvokePromptAsync<string>(prompt, new KernelArguments() { ["textoAResumir"] = textoAResumir });
        }
    }
}
