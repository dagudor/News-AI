using Microsoft.SemanticKernel;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;
using NewsAI.API.Models;

namespace NewsAI.Negocio.Services
{
    public class RssReaderService
    {
        private readonly Kernel kernel;
        public RssReaderService(Kernel kernel)
        {
            this.kernel = kernel;
        }

        public async Task<List<RssNewsItem>> LeerRssYAplicarIAResumen(string url) 
        { 
            var noticias = new List<RssNewsItem>();

            using var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            if (feed != null)
            {
                var funcionResumen = kernel.Plugins.GetFunction("SummarizePlugin", "Summarize");
                foreach (var item in feed.Items.Take(1))
                {
                    string textoOriginal = item.Content?.ToString().Trim() 
                        ?? item.Summary?.Text?.Trim()
                        ?? string.Empty;

                    textoOriginal = System.Text.RegularExpressions.Regex.Replace(textoOriginal, "<.*?>", "");
                    textoOriginal = WebUtility.HtmlDecode(textoOriginal);

                    textoOriginal = @"Un técnico de Seguridad y Control del Centro de Coordinación de Emergencias de la Generalitat que participó en el envío de la alerta masiva a móviles para avisar a la población del peligro de la dana —que el pasado 29 de octubre dejó 228 muertos en la provincia de Valencia— sostiene que la exconsejera de Justicia e Interior del Gobierno de Carlos Mazón, Salomé Pradas, le indicó “que no mandara nada hasta que ella le diera el visto bueno”. Así lo indican fuentes presentes en la declaración de R. E., que este martes ha comparecido como testigo ante el Juzgado de Instrucción número 3 de Catarroja (Valencia), que indaga penalmente la riada.";
                    textoOriginal = @"El 22 de abril, Pedro Sánchez anunció el plan del Gobierno para alcanzar el 2% del PIB en gasto militar en 2025. 10.471 millones de euros divididos en varias partidas. El Ejecutivo ha remarcado que el paquete potencia sobre todo la inversión en ""nuevas capacidades de telecomunicaciones y ciberseguridad"", pero hasta un 19% se dedicará a comprar o fabricar nuevos equipos de defensa y disuasión. Las izquierdas del Congreso mostraron su oposición rotunda y el rearme acordado en Consejo de Ministros dominó la conversación pública durante unos días, hasta que el gran apagón del lunes 28 de abril sorprendió a todo el país.

Todas las fuentes consultadas coinciden en la importancia de que la parte de la comparecencia de este miércoles del presidente del Gobierno sobre el apagón —del que todavía se desconocen las causas— sirva para hacer extensible a la ciudadanía cuando menos los datos que el Ejecutivo sí conoce y, además, para ""exigir responsabilidades tanto políticas, como de la empresa privada"", tal y como ha enfatizado Àgueda Micó, portavoz de Compromís en el grupo parlamentario de Sumar. 

Pero existe una cierta sensación entre las izquierdas de que Sánchez pueda jugar sus cartas de tal manera que el peso de la jornada recaiga en el apagón, relegando a un segundo plano el aumento del gasto militar, y no quieren que se pierda de vista que la comparecencia, tal y como refleja el orden del día del Pleno, versa sobre ambos temas.";
                    if (string.IsNullOrEmpty(textoOriginal)) 
                        continue;
                    var variables = new KernelArguments
                    {
                        ["textoAResumir"] = textoOriginal
                    };

                    var resultado = await funcionResumen.InvokeAsync(kernel, variables);

                    string resumen = resultado.GetValue<string>() ?? "Sin resumen";

                    noticias.Add(new RssNewsItem
                    {
                        Titulo = item.Title?.Text,
                        Resumen = resumen,
                        Link = item.Links.FirstOrDefault().Uri.ToString(),
                        FechaPublicacion = item.PublishDate.DateTime

                    });
                }
            }
            return noticias;
        }
        public async Task<List<RssNewsItem>> LeerRss(string url)
        {
            var noticias = new List<RssNewsItem>();

            using var reader = XmlReader.Create(url);
            var feed = SyndicationFeed.Load(reader);
            if(feed != null)
            {
                foreach (var item in feed.Items.Take(5))
                {
                    noticias.Add(new RssNewsItem
                    {
                        Titulo = item.Title?.Text,
                        Resumen = item.Summary?.Text,
                        Link = item.Links.FirstOrDefault().Uri.ToString(),
                        FechaPublicacion = item.PublishDate.DateTime

                    });
                }
            }

            return noticias;
        }
    }
}
