using System.ServiceModel.Syndication;
using System.Xml;
using System.Text.RegularExpressions;
using NewsAI.Negocio.Interfaces;
using NewsAI.Dominio.Entidades;
using Microsoft.Playwright;

namespace NewsAI.Negocio.Services
{
    public class NoticiasExtractorService : INoticiasExtractorService
    {
        private readonly ILogger<NoticiasExtractorService> logger;
        private readonly HttpClient httpClient;

        public NoticiasExtractorService(ILogger<NoticiasExtractorService> logger, HttpClient httpClient)
        {
            this.logger = logger;
            this.httpClient = httpClient;
            ConfigurarHttpClient();
        }

        private void ConfigurarHttpClient()
        {
            httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept",
                "application/rss+xml, application/xml, text/xml, text/html, */*");
            httpClient.DefaultRequestHeaders.Add("Accept-Language", "es-ES,es;q=0.9,en;q=0.8");
            httpClient.DefaultRequestHeaders.Add("Cache-Control", "no-cache");

            httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<List<NoticiaExtraida>> ExtraerNoticiasAsync(string url, int limite = 30)
        {
            try
            {
                logger.LogInformation($"🌐 EXTRACCIÓN UNIVERSAL INICIADA: {url}");

                // ESTRATEGIA 1: RSS Universal (la más eficiente)
                var noticiasRss = await ExtraerRSSUniversal(url, limite);
                if (noticiasRss.Any())
                {
                    logger.LogInformation($"✅ RSS UNIVERSAL EXITOSO: {noticiasRss.Count} noticias extraídas");
                    return noticiasRss;
                }

                // ESTRATEGIA 2: Playwright Universal (la más potente)
                var noticiasPlaywright = await ExtraerConPlaywrightUniversal(url, limite);
                if (noticiasPlaywright.Any())
                {
                    logger.LogInformation($"✅ PLAYWRIGHT UNIVERSAL EXITOSO: {noticiasPlaywright.Count} noticias extraídas");
                    return noticiasPlaywright;
                }

                // ESTRATEGIA 3: HTTP + Regex Universal (fallback)
                var noticiasHttp = await ExtraerConHttpUniversal(url, limite);
                if (noticiasHttp.Any())
                {
                    logger.LogInformation($"✅ HTTP UNIVERSAL EXITOSO: {noticiasHttp.Count} noticias extraídas");
                    return noticiasHttp;
                }

                logger.LogWarning($"❌ TODAS LAS ESTRATEGIAS FALLARON para {url}");
                return new List<NoticiaExtraida>();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ ERROR CRÍTICO extrayendo de {url}");
                return new List<NoticiaExtraida>();
            }
        }

        public async Task<List<NoticiaExtraida>> ExtraerNoticiasRSSAsync(string urlRss, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                logger.LogInformation($"Intentando extraer RSS de: {urlRss}");

                var response = await httpClient.GetAsync(urlRss);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning($" Error HTTP {response.StatusCode}: {response.ReasonPhrase}");
                    return noticias;
                }

                var xmlContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(xmlContent))
                {
                    logger.LogWarning(" Contenido RSS vacío");
                    return noticias;
                }

                logger.LogInformation($" Contenido RSS obtenido. Longitud: {xmlContent.Length} caracteres");

                //  LIMPIAR XML PROBLEMÁTICO
                xmlContent = LimpiarXmlCorrupto(xmlContent);

                var xmlReaderSettings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Ignore,
                    XmlResolver = null,
                    CheckCharacters = false,
                    IgnoreWhitespace = true,
                    IgnoreComments = true,
                    CloseInput = true
                };

                using var stringReader = new StringReader(xmlContent);
                using var xmlReader = XmlReader.Create(stringReader, xmlReaderSettings);

                var feed = SyndicationFeed.Load(xmlReader);

                logger.LogInformation($"Feed cargado: {feed.Title?.Text}");
                logger.LogInformation($"Items disponibles: {feed.Items.Count()}");

                //  LOGS DETALLADOS - VERIFICAR QUE TOMA EL LÍMITE CORRECTO
                logger.LogInformation($"LÍMITE SOLICITADO: {limite}");
                logger.LogInformation($"ITEMS A PROCESAR: {Math.Min(limite, feed.Items.Count())}");

                int itemsProcessados = 0;
                int itemsValidos = 0;
                int itemsInvalidos = 0;

                foreach (var item in feed.Items.Take(limite)) //  AQUÍ DEBE ESTAR EL LÍMITE CORRECTO
                {
                    itemsProcessados++;

                    try
                    {
                        logger.LogDebug($"Procesando item {itemsProcessados}/{limite}");

                        var contenido = ExtraerContenidoCompleto(item);

                        var noticia = new NoticiaExtraida
                        {
                            Titulo = LimpiarTexto(item.Title?.Text) ?? "Sin título",
                            Contenido = contenido,
                            Url = item.Links?.FirstOrDefault()?.Uri?.ToString() ?? urlRss,
                            FechaPublicacion = item.PublishDate.DateTime != DateTime.MinValue
                                ? item.PublishDate.DateTime
                                : DateTime.Now,
                            Fuente = LimpiarTexto(feed.Title?.Text) ?? ExtraerNombreFuente(urlRss),
                            Hashtags = new List<string>()
                        };

                        //  VALIDAR PERO CON LOGS
                        if (EsNoticiaValida(noticia))
                        {
                            noticias.Add(noticia);
                            itemsValidos++;
                            logger.LogDebug($" Item {itemsProcessados} VÁLIDO: {noticia.Titulo.Substring(0, Math.Min(50, noticia.Titulo.Length))}...");
                        }
                        else
                        {
                            itemsInvalidos++;
                            logger.LogDebug($" Item {itemsProcessados} INVÁLIDO: {noticia.Titulo} (Título: {noticia.Titulo.Length} chars, Contenido: {noticia.Contenido.Length} chars)");
                        }
                    }
                    catch (Exception ex)
                    {
                        itemsInvalidos++;
                        logger.LogWarning($"Error procesando item {itemsProcessados}: {ex.Message}");
                    }
                }

                //  RESUMEN DETALLADO
                logger.LogInformation($"RESUMEN EXTRACCIÓN:");
                logger.LogInformation($"Items disponibles en feed: {feed.Items.Count()}");
                logger.LogInformation($"Límite solicitado: {limite}");
                logger.LogInformation($"Items procesados: {itemsProcessados}");
                logger.LogInformation($"Items válidos: {itemsValidos}");
                logger.LogInformation($"Items inválidos: {itemsInvalidos}");
                logger.LogInformation($"Noticias finales: {noticias.Count}");

                logger.LogInformation($" Extraídas {noticias.Count} noticias del RSS exitosamente");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $" Error extrayendo RSS de {urlRss}: {ex.Message}");

                // Fallback manual si es necesario
                try
                {
                    logger.LogInformation("Intentando parseo manual del RSS...");
                    var noticiasManual = await ParsearRssManualmente(urlRss, limite);
                    if (noticiasManual.Any())
                    {
                        logger.LogInformation($" Parseo manual exitoso: {noticiasManual.Count} noticias");
                        return noticiasManual;
                    }
                }
                catch (Exception fallbackEx)
                {
                    logger.LogError(fallbackEx, " También falló el parseo manual");
                }
            }

            return noticias;
        }

        /// <summary>
        /// Este metodo extrae noticias con playwritgt para scrapping
        /// /// </summary>
        /// <param name="url"></param>
        /// <param name="limite"></param>
        /// <returns></returns>
        public async Task<List<NoticiaExtraida>> ExtraerConPlaywrightAsync(string url, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                logger.LogInformation($"Iniciando Playwright para: {url}");

                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                });

                var page = await context.NewPageAsync();

                // Configurar timeout
                page.SetDefaultTimeout(30000);

                // Navegar a la página
                var response = await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 30000
                });

                if (response?.Status != 200)
                {
                    logger.LogWarning($" Playwright HTTP {response?.Status}");
                    return noticias;
                }

                // Esperar a que cargue el contenido
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions
                {
                    Timeout = 15000
                });

                logger.LogInformation($" Página cargada con Playwright");

                //  EXTRAER NOTICIAS CON SELECTORES DINÁMICOS
                var noticiasEncontradas = await ExtraerNoticiasConSelectores(page, limite);

                foreach (var noticia in noticiasEncontradas)
                {
                    noticia.Fuente = ExtraerNombreFuente(url);
                    noticia.Hashtags = new List<string>(); // Se generarán en el clasificador
                    noticias.Add(noticia);
                }

                logger.LogInformation($" Playwright extrajo {noticias.Count} noticias");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $" Error en Playwright para {url}");
            }

            return noticias;
        }

        private async Task<List<NoticiaExtraida>> ExtraerNoticiasConSelectores(IPage page, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                //  SELECTORES COMUNES PARA NOTICIAS
                var selectores = new[]
                {
                    "article",
                    ".noticia",
                    ".news-item",
                    ".post",
                    ".entry",
                    ".story",
                    "[class*='article']",
                    "[class*='news']",
                    "[class*='story']",
                    "[class*='post']",
                    "h2 a, h3 a", // Enlaces en títulos
                    ".headline a"
                };

                foreach (var selector in selectores)
                {
                    try
                    {
                        var elementos = await page.QuerySelectorAllAsync(selector);

                        if (elementos.Any())
                        {
                            logger.LogInformation($"Encontrados {elementos.Count} elementos con selector: {selector}");

                            for (int i = 0; i < Math.Min(limite - noticias.Count, elementos.Count); i++)
                            {
                                var elemento = elementos[i];

                                var titulo = await ExtraerTexto(elemento, "h1, h2, h3, h4, .title, .headline, [class*='title'], [class*='headline']");
                                var contenido = await ExtraerTexto(elemento, "p, .content, .summary, .excerpt, .description, [class*='content'], [class*='summary']");
                                var enlace = await ExtraerEnlace(elemento, "a");

                                if (!string.IsNullOrEmpty(titulo) && titulo.Length > 10)
                                {
                                    noticias.Add(new NoticiaExtraida
                                    {
                                        Titulo = titulo.Trim(),
                                        Contenido = !string.IsNullOrEmpty(contenido) ? contenido.Trim() : titulo,
                                        Url = enlace ?? page.Url,
                                        FechaPublicacion = DateTime.Now,
                                        Fuente = "Web Scraping",
                                        Hashtags = new List<string>()
                                    });

                                    logger.LogDebug($" Noticia Playwright: {titulo.Substring(0, Math.Min(50, titulo.Length))}...");
                                }
                            }

                            if (noticias.Count >= limite) break;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug($"Selector {selector} falló: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error extrayendo con selectores");
            }

            return noticias.Take(limite).ToList();
        }

        public async Task<List<NoticiaExtraida>> ExtraerNoticiasWebAsync(string urlWeb, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                logger.LogInformation($"Intentando scraping web básico de: {urlWeb}");

                var response = await httpClient.GetAsync(urlWeb);

                if (!response.IsSuccessStatusCode)
                {
                    logger.LogWarning($" Error HTTP {response.StatusCode}: {response.ReasonPhrase}");
                    return noticias;
                }

                var htmlContent = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(htmlContent))
                {
                    logger.LogWarning(" Contenido HTML vacío");
                    return noticias;
                }

                logger.LogInformation($" Contenido HTML obtenido. Longitud: {htmlContent.Length} caracteres");

                // Scraping básico
                var titulos = ExtraerTitulosHTML(htmlContent);
                var contenidos = ExtraerContenidosHTML(htmlContent);

                for (int i = 0; i < Math.Min(limite, Math.Min(titulos.Count, contenidos.Count)); i++)
                {
                    noticias.Add(new NoticiaExtraida
                    {
                        Titulo = titulos[i],
                        Contenido = LimpiarTexto(contenidos[i]),
                        Url = urlWeb,
                        FechaPublicacion = DateTime.Now,
                        Fuente = ExtraerNombreFuente(urlWeb),
                        Hashtags = new List<string>()
                    });
                }

                logger.LogInformation($" Extraídas {noticias.Count} noticias del sitio web");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $" Error extrayendo web: {ex.Message}");
            }

            return noticias;
        }

        private async Task<string> ExtraerTexto(IElementHandle elemento, string selector)
        {
            try
            {
                var subelemento = await elemento.QuerySelectorAsync(selector);
                if (subelemento != null)
                {
                    var texto = await subelemento.TextContentAsync();
                    return LimpiarTexto(texto);
                }

                // Si no encuentra subelemento, extraer texto del elemento principal
                var textoElemento = await elemento.TextContentAsync();
                return LimpiarTexto(textoElemento);
            }
            catch
            {
                return string.Empty;
            }
        }

        private async Task<string> ExtraerEnlace(IElementHandle elemento, string selector)
        {
            try
            {
                var enlaceElemento = await elemento.QuerySelectorAsync(selector);
                if (enlaceElemento != null)
                {
                    var href = await enlaceElemento.GetAttributeAsync("href");
                    return href;
                }

                // Si el elemento actual es un enlace
                var hrefDirecto = await elemento.GetAttributeAsync("href");
                return hrefDirecto;
            }
            catch
            {
                return null;
            }
        }

        private string ExtraerContenidoCompleto(SyndicationItem item)
        {
            var contenidos = new List<string>();

            try
            {
                // Intentar obtener contenido de múltiples fuentes
                if (item.Summary?.Text != null)
                    contenidos.Add(item.Summary.Text);

                if (item.Content != null)
                    contenidos.Add(item.Content.ToString());

                foreach (var extension in item.ElementExtensions)
                {
                    try
                    {
                        if (extension.OuterName.ToLower().Contains("content") ||
                            extension.OuterName.ToLower().Contains("description") ||
                            extension.OuterName.ToLower().Contains("encoded"))
                        {
                            var valor = extension.GetObject<string>();
                            if (!string.IsNullOrEmpty(valor))
                                contenidos.Add(valor);
                        }
                    }
                    catch
                    {
                        // Ignorar errores de extensiones específicas
                    }
                }

                var contenidoCompleto = string.Join(" ", contenidos);
                return LimpiarTexto(contenidoCompleto);
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error extrayendo contenido completo: {ex.Message}");
                return item.Title?.Text ?? "Sin contenido disponible";
            }
        }

        private List<string> ExtraerTitulosHTML(string html)
        {
            var titulos = new List<string>();

            var patterns = new[]
            {
                @"<h[1-4][^>]*>(.*?)</h[1-4]>",
                @"<title[^>]*>(.*?)</title>",
                @"class=[""'].*?title.*?[""'][^>]*>(.*?)</[^>]+>",
                @"class=[""'].*?headline.*?[""'][^>]*>(.*?)</[^>]+>",
                @"<a[^>]*class=[""'].*?headline.*?[""'][^>]*>(.*?)</a>"
            };

            foreach (var pattern in patterns)
            {
                try
                {
                    var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    foreach (Match match in matches)
                    {
                        var titulo = Regex.Replace(match.Groups[1].Value, "<.*?>", string.Empty).Trim();
                        if (!string.IsNullOrEmpty(titulo) && titulo.Length > 10 && titulo.Length < 200)
                        {
                            titulos.Add(titulo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug($"Error con pattern {pattern}: {ex.Message}");
                }
            }

            return titulos.Distinct().Take(20).ToList();
        }

        private List<string> ExtraerContenidosHTML(string html)
        {
            var contenidos = new List<string>();

            var patterns = new[]
            {
                @"<p[^>]*>(.*?)</p>",
                @"<div[^>]*class=[""'].*?content.*?[""'][^>]*>(.*?)</div>",
                @"<article[^>]*>(.*?)</article>",
                @"class=[""'].*?summary.*?[""'][^>]*>(.*?)</[^>]+>",
                @"class=[""'].*?excerpt.*?[""'][^>]*>(.*?)</[^>]+>"
            };

            foreach (var pattern in patterns)
            {
                try
                {
                    var matches = Regex.Matches(html, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    foreach (Match match in matches)
                    {
                        var contenido = Regex.Replace(match.Groups[1].Value, "<.*?>", string.Empty).Trim();
                        if (!string.IsNullOrEmpty(contenido) && contenido.Length > 50 && contenido.Length < 1000)
                        {
                            contenidos.Add(contenido);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug($"Error con pattern de contenido {pattern}: {ex.Message}");
                }
            }

            return contenidos.Distinct().Take(20).ToList();
        }

        private string LimpiarTexto(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "Sin contenido disponible";

            try
            {
                // Limpiar HTML y normalizar texto
                var limpio = Regex.Replace(texto, "<.*?>", " ");
                limpio = Regex.Replace(limpio, @"\s+", " ");
                limpio = limpio.Replace("&amp;", "&")
                              .Replace("&lt;", "<")
                              .Replace("&gt;", ">")
                              .Replace("&quot;", "\"")
                              .Replace("&#39;", "'")
                              .Replace("&nbsp;", " ")
                              .Replace("&apos;", "'")
                              .Replace("&copy;", "©");

                return limpio.Trim();
            }
            catch
            {
                return texto.Trim();
            }
        }

        private string ExtraerNombreFuente(string url)
        {
            try
            {
                var uri = new Uri(url);
                var host = uri.Host.Replace("www.", "");
                return char.ToUpper(host[0]) + host.Substring(1);
            }
            catch
            {
                return "Fuente desconocida";
            }
        }

        private void LogearNoticiasExtraidas(List<NoticiaExtraida> noticias, string metodo)
        {
            logger.LogInformation($"NOTICIAS EXTRAÍDAS ({metodo}):");
            for (int i = 0; i < Math.Min(3, noticias.Count); i++)
            {
                var noticia = noticias[i];
                logger.LogInformation($"   {i + 1}. '{noticia.Titulo}' - Fuente: {noticia.Fuente}");
                logger.LogInformation($"Contenido: {noticia.Contenido.Substring(0, Math.Min(150, noticia.Contenido.Length))}...");
                logger.LogInformation($"URL: {noticia.Url}");
            }

            if (noticias.Count > 3)
            {
                logger.LogInformation($"   ... y {noticias.Count - 3} noticias más");
            }
        }

        //  MÉTODO PARA LIMPIAR XML CORRUPTO
        private string LimpiarXmlCorrupto(string xmlContent)
        {
            try
            {
                //  LIMPIAR ENTIDADES HTML - COMILLAS CORREGIDAS
                xmlContent = xmlContent
                    .Replace("&nbsp;", " ")
                    .Replace("&mdash;", "—")
                    .Replace("&ndash;", "–")
                    .Replace("&hellip;", "…")
                    .Replace("&rsquo;", "'")
                    .Replace("&lsquo;", "'")
                    .Replace("&rdquo;", "\"")  //  Comilla doble correcta
                    .Replace("&ldquo;", "\""); //  Comilla doble correcta

                //  ELIMINAR CARACTERES DE CONTROL INVÁLIDOS
                var caracteresInvalidos = new char[]
                {
            '\x00', '\x01', '\x02', '\x03', '\x04', '\x05', '\x06', '\x07', '\x08',
            '\x0B', '\x0C', '\x0E', '\x0F', '\x10', '\x11', '\x12', '\x13', '\x14',
            '\x15', '\x16', '\x17', '\x18', '\x19', '\x1A', '\x1B', '\x1C', '\x1D',
            '\x1E', '\x1F'
                };

                foreach (char c in caracteresInvalidos)
                {
                    xmlContent = xmlContent.Replace(c.ToString(), "");
                }

                logger.LogDebug(" XML limpiado de caracteres problemáticos");
                return xmlContent;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Error limpiando XML: {ex.Message}");
                return xmlContent; // Devolver original si falla la limpieza
            }
        }



        //  Parseo manual si no hay otra opcion
        private async Task<List<NoticiaExtraida>> ParsearRssManualmente(string urlRss, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                var response = await httpClient.GetAsync(urlRss);
                var xmlContent = await response.Content.ReadAsStringAsync();

                // Limpiar XML
                xmlContent = LimpiarXmlCorrupto(xmlContent);

                // Usar regex para extraer items básicos
                var patronItem = @"<item>(.*?)</item>";
                var patronTitulo = @"<title><!\[CDATA\[(.*?)\]\]></title>|<title>(.*?)</title>";
                var patronDescripcion = @"<description><!\[CDATA\[(.*?)\]\]></description>|<description>(.*?)</description>";
                var patronLink = @"<link>(.*?)</link>";
                var patronFecha = @"<pubDate>(.*?)</pubDate>";

                var matchesItems = Regex.Matches(xmlContent, patronItem, RegexOptions.Singleline | RegexOptions.IgnoreCase);

                foreach (Match matchItem in matchesItems.Take(limite))
                {
                    try
                    {
                        var itemContent = matchItem.Groups[1].Value;

                        var matchTitulo = Regex.Match(itemContent, patronTitulo, RegexOptions.Singleline);
                        var matchDescripcion = Regex.Match(itemContent, patronDescripcion, RegexOptions.Singleline);
                        var matchLink = Regex.Match(itemContent, patronLink);
                        var matchFecha = Regex.Match(itemContent, patronFecha);

                        var titulo = matchTitulo.Success ?
                            (matchTitulo.Groups[1].Success ? matchTitulo.Groups[1].Value : matchTitulo.Groups[2].Value) :
                            "Sin título";

                        var descripcion = matchDescripcion.Success ?
                            (matchDescripcion.Groups[1].Success ? matchDescripcion.Groups[1].Value : matchDescripcion.Groups[2].Value) :
                            "Sin descripción";

                        var noticia = new NoticiaExtraida
                        {
                            Titulo = LimpiarTexto(titulo),
                            Contenido = LimpiarTexto(descripcion),
                            Url = matchLink.Success ? matchLink.Groups[1].Value.Trim() : urlRss,
                            FechaPublicacion = ParsearFecha(matchFecha.Success ? matchFecha.Groups[1].Value : null) ?? DateTime.Now,
                            Fuente = ExtraerNombreFuente(urlRss),
                            Hashtags = new List<string>()
                        };

                        if (EsNoticiaValida(noticia))
                        {
                            noticias.Add(noticia);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogDebug($"Error procesando item manual: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en parseo manual");
            }

            return noticias;
        }

        //  VALIDACIÓN DE NOTICIA
        private bool EsNoticiaValida(NoticiaExtraida noticia)
        {
            //  VALIDACIÓN MÁS PERMISIVA PARA OBTENER MÁS NOTICIAS
            var esValida = !string.IsNullOrWhiteSpace(noticia.Titulo) &&
                   noticia.Titulo.Length >= 5 && //  Reducido de 10 a 5
                   !string.IsNullOrWhiteSpace(noticia.Contenido) &&
                   noticia.Contenido.Length >= 10 && //  Reducido de 20 a 10
                   !noticia.Titulo.ToLower().Contains("cookie") &&
                   !noticia.Titulo.ToLower().Contains("política") &&
                   !noticia.Titulo.ToLower().Contains("aviso legal") &&
                   !noticia.Titulo.ToLower().Contains("términos") &&
                   !noticia.Titulo.ToLower().StartsWith("error") &&
                   !noticia.Titulo.Equals("Sin título");

            return esValida;
        }

        //  parsing de fechas en diferentes formartos
        private DateTime? ParsearFecha(string fechaTexto)
        {
            if (string.IsNullOrWhiteSpace(fechaTexto))
                return DateTime.Now;

            var formatosFecha = new[]
            {
        "ddd, dd MMM yyyy HH:mm:ss zzz",
        "ddd, dd MMM yyyy HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssZ",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "dd/MM/yyyy HH:mm:ss",
        "MM/dd/yyyy HH:mm:ss"
    };

            foreach (var formato in formatosFecha)
            {
                if (DateTime.TryParseExact(fechaTexto, formato, null, System.Globalization.DateTimeStyles.None, out var fecha))
                {
                    return fecha;
                }
            }

            if (DateTime.TryParse(fechaTexto, out var fechaGenerica))
            {
                return fechaGenerica;
            }

            return DateTime.Now;
        }

        // ESTRATEGIA 1: RSS UNIVERSAL - DETECTA AUTOMÁTICAMENTE FEEDS RSS
        private async Task<List<NoticiaExtraida>> ExtraerRSSUniversal(string url, int limite)
        {
            var urlsRssPosibles = GenerarUrlsRssPosibles(url);

            foreach (var urlRss in urlsRssPosibles)
            {
                try
                {
                    logger.LogDebug($"🔍 Intentando RSS: {urlRss}");
                    var noticias = await ExtraerNoticiasRSSAsync(urlRss, limite);

                    if (noticias.Any())
                    {
                        logger.LogInformation($"✅ RSS encontrado en: {urlRss}");
                        return noticias;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug($"❌ RSS falló {urlRss}: {ex.Message}");
                }
            }

            return new List<NoticiaExtraida>();
        }

        // GENERAR URLs RSS POSIBLES PARA CUALQUIER WEB
        private List<string> GenerarUrlsRssPosibles(string url)
        {
            var baseUrl = GetBaseUrl(url);
            var urlsRss = new List<string>
    {
        url, // URL original por si es directamente RSS
        $"{baseUrl}/rss",
        $"{baseUrl}/rss.xml",
        $"{baseUrl}/feed",
        $"{baseUrl}/feed.xml",
        $"{baseUrl}/feeds",
        $"{baseUrl}/feeds.xml",
        $"{baseUrl}/index.xml",
        $"{baseUrl}/rss/all",
        $"{baseUrl}/rss/news",
        $"{baseUrl}/rss/noticias",
        $"{baseUrl}/feed/all",
        $"{baseUrl}/feed/news",
        $"{baseUrl}/api/rss",
        $"{baseUrl}/news/rss",
        $"{baseUrl}/noticias/rss",
        $"{baseUrl}/actualidad/rss",
        $"{baseUrl}/rss/actualidad",
        $"{baseUrl}/rss/portada"
    };

            return urlsRss;
        }

        // ESTRATEGIA 2: PLAYWRIGHT UNIVERSAL - SELECTORES INTELIGENTES
        private async Task<List<NoticiaExtraida>> ExtraerConPlaywrightUniversal(string url, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                using var playwright = await Playwright.CreateAsync();
                await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
                {
                    Headless = true,
                    Args = new[] { "--no-sandbox", "--disable-setuid-sandbox", "--disable-blink-features=AutomationControlled" }
                });

                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    UserAgent = GetRandomUserAgent(),
                    ViewportSize = new ViewportSize { Width = 1920, Height = 1080 }
                });

                var page = await context.NewPageAsync();

                // CONFIGURAR TIMEOUTS RAZONABLES
                page.SetDefaultTimeout(20000);

                logger.LogInformation($"🎭 Cargando página con Playwright: {url}");

                await page.GotoAsync(url, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 20000
                });

                // ESPERAR A QUE CARGUE CONTENIDO DINÁMICO
                try
                {
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 10000 });
                }
                catch
                {
                    // No es crítico si no hay estado de red idle
                }

                logger.LogInformation("📄 Página cargada, iniciando extracción universal");

                // EXTRACCIÓN CON SELECTORES UNIVERSALES INTELIGENTES
                noticias = await ExtraerNoticiasConSelectoresUniversales(page, limite, url);

                logger.LogInformation($"🎯 Playwright Universal extrajo {noticias.Count} noticias");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ Error en Playwright Universal para {url}");
            }

            return noticias;
        }

        // SELECTORES UNIVERSALES - FUNCIONAN EN 90% DE WEBS DE NOTICIAS
        private async Task<List<NoticiaExtraida>> ExtraerNoticiasConSelectoresUniversales(IPage page, int limite, string baseUrl)
        {
            var noticias = new List<NoticiaExtraida>();

            // SELECTORES UNIVERSALES ORDENADOS POR EFECTIVIDAD
            var estrategiasSelectores = new[]
            {
        // ESTRATEGIA 1: Elementos article (HTML5 semántico - más común)
        new SelectorStrategy
        {
            Selectores = new[] { "article" },
            TituloSelectores = new[] { "h1", "h2", "h3", "h4", ".title", ".headline", "[class*='title']", "[class*='headline']" },
            ContenidoSelectores = new[] { "p", ".summary", ".excerpt", ".description", ".content", "[class*='summary']", "[class*='excerpt']" },
            EnlaceSelectores = new[] { "a[href]", ".link" },
            Nombre = "Article HTML5"
        },

        // ESTRATEGIA 2: Enlaces de títulos (muy universal)
        new SelectorStrategy
        {
            Selectores = new[] { "h1 a", "h2 a", "h3 a", "h4 a" },
            EsEnlaceDirecto = true,
            Nombre = "Enlaces de Títulos"
        },

        // ESTRATEGIA 3: Clases comunes de noticias
        new SelectorStrategy
        {
            Selectores = new[] { ".news-item", ".noticia", ".post", ".entry", ".story", ".article-item" },
            TituloSelectores = new[] { "h1", "h2", "h3", "h4", ".title", ".headline" },
            ContenidoSelectores = new[] { "p", ".summary", ".excerpt", ".description" },
            EnlaceSelectores = new[] { "a[href]" },
            Nombre = "Clases Comunes"
        },

        // ESTRATEGIA 4: Contenedores con patrones
        new SelectorStrategy
        {
            Selectores = new[] { "[class*='news']", "[class*='article']", "[class*='story']", "[class*='post']" },
            TituloSelectores = new[] { "h1", "h2", "h3", "h4", "a" },
            ContenidoSelectores = new[] { "p", ".text", ".content" },
            EnlaceSelectores = new[] { "a[href]" },
            Nombre = "Patrones de Clases"
        },

        // ESTRATEGIA 5: Listas de noticias
        new SelectorStrategy
        {
            Selectores = new[] { "ul li", "ol li", ".list li", ".news-list li" },
            TituloSelectores = new[] { "a", "h1", "h2", "h3", "h4" },
            ContenidoSelectores = new[] { "p", ".text" },
            EnlaceSelectores = new[] { "a[href]" },
            Nombre = "Listas"
        }
    };

            foreach (var estrategia in estrategiasSelectores)
            {
                try
                {
                    var noticiasEstrategia = await EjecutarEstrategiaSelector(page, estrategia, limite - noticias.Count, baseUrl);

                    if (noticiasEstrategia.Any())
                    {
                        noticias.AddRange(noticiasEstrategia);
                        logger.LogInformation($"✅ {estrategia.Nombre}: {noticiasEstrategia.Count} noticias extraídas");

                        if (noticias.Count >= limite) break;
                    }
                    else
                    {
                        logger.LogDebug($"❌ {estrategia.Nombre}: 0 noticias");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogDebug($"❌ Error en estrategia {estrategia.Nombre}: {ex.Message}");
                }
            }

            return noticias.Take(limite).ToList();
        }

        // EJECUTAR UNA ESTRATEGIA DE SELECTOR ESPECÍFICA
        private async Task<List<NoticiaExtraida>> EjecutarEstrategiaSelector(IPage page, SelectorStrategy estrategia, int limite, string baseUrl)
        {
            var noticias = new List<NoticiaExtraida>();

            foreach (var selector in estrategia.Selectores)
            {
                try
                {
                    var elementos = await page.QuerySelectorAllAsync(selector);
                    logger.LogDebug($"🔍 Selector '{selector}': {elementos.Count} elementos encontrados");

                    if (!elementos.Any()) continue;

                    foreach (var elemento in elementos.Take(limite - noticias.Count))
                    {
                        try
                        {
                            var noticia = await ExtraerNoticiaDeElemento(elemento, estrategia, baseUrl);

                            if (noticia != null && EsNoticiaValidaUniversal(noticia))
                            {
                                noticias.Add(noticia);
                                logger.LogDebug($"✅ Extraída: {noticia.Titulo.Substring(0, Math.Min(50, noticia.Titulo.Length))}...");
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug($"Error extrayendo elemento: {ex.Message}");
                        }

                        if (noticias.Count >= limite) break;
                    }

                    if (noticias.Count >= limite) break;
                }
                catch (Exception ex)
                {
                    logger.LogDebug($"Error con selector {selector}: {ex.Message}");
                }
            }

            return noticias;
        }

        // EXTRAER NOTICIA DE UN ELEMENTO ESPECÍFICO
        private async Task<NoticiaExtraida?> ExtraerNoticiaDeElemento(IElementHandle elemento, SelectorStrategy estrategia, string baseUrl)
        {
            try
            {
                string titulo = "";
                string contenido = "";
                string enlace = "";

                if (estrategia.EsEnlaceDirecto)
                {
                    // Para enlaces directos (h1 a, h2 a, etc.)
                    titulo = await elemento.TextContentAsync() ?? "";
                    enlace = await elemento.GetAttributeAsync("href") ?? "";
                    contenido = titulo; // Usar título como contenido si no hay más información
                }
                else
                {
                    // Para elementos contenedores
                    titulo = await ExtraerConSelectoresMultiples(elemento, estrategia.TituloSelectores);
                    contenido = await ExtraerConSelectoresMultiples(elemento, estrategia.ContenidoSelectores);
                    enlace = await ExtraerEnlaceConSelectores(elemento, estrategia.EnlaceSelectores);
                }

                if (string.IsNullOrWhiteSpace(titulo) || titulo.Length < 10)
                {
                    return null;
                }

                return new NoticiaExtraida
                {
                    Titulo = LimpiarTextoUniversal(titulo),
                    Contenido = LimpiarTextoUniversal(string.IsNullOrEmpty(contenido) ? titulo : contenido),
                    Url = ConstruirUrlCompleta(enlace, baseUrl),
                    FechaPublicacion = DateTime.Now,
                    Fuente = ExtraerNombreFuente(baseUrl),
                    Hashtags = new List<string>()
                };
            }
            catch (Exception ex)
            {
                logger.LogDebug($"Error extrayendo noticia de elemento: {ex.Message}");
                return null;
            }
        }

        // EXTRAER TEXTO CON MÚLTIPLES SELECTORES
        private async Task<string> ExtraerConSelectoresMultiples(IElementHandle elemento, string[] selectores)
        {
            foreach (var selector in selectores)
            {
                try
                {
                    var subelemento = await elemento.QuerySelectorAsync(selector);
                    if (subelemento != null)
                    {
                        var texto = await subelemento.TextContentAsync();
                        if (!string.IsNullOrWhiteSpace(texto) && texto.Length > 5)
                        {
                            return texto.Trim();
                        }
                    }
                }
                catch
                {
                    // Continuar con el siguiente selector
                }
            }

            // Si no encuentra subelemento, intentar texto del elemento principal
            try
            {
                var textoElemento = await elemento.TextContentAsync();
                return textoElemento?.Trim() ?? "";
            }
            catch
            {
                return "";
            }
        }

        // EXTRAER ENLACE CON MÚLTIPLES SELECTORES
        private async Task<string> ExtraerEnlaceConSelectores(IElementHandle elemento, string[] selectores)
        {
            foreach (var selector in selectores)
            {
                try
                {
                    var enlaceElemento = await elemento.QuerySelectorAsync(selector);
                    if (enlaceElemento != null)
                    {
                        var href = await enlaceElemento.GetAttributeAsync("href");
                        if (!string.IsNullOrWhiteSpace(href))
                        {
                            return href;
                        }
                    }
                }
                catch
                {
                    // Continuar con el siguiente selector
                }
            }

            // Verificar si el elemento actual es un enlace
            try
            {
                var hrefDirecto = await elemento.GetAttributeAsync("href");
                return hrefDirecto ?? "";
            }
            catch
            {
                return "";
            }
        }

        // VALIDACIÓN UNIVERSAL DE NOTICIAS
        private bool EsNoticiaValidaUniversal(NoticiaExtraida noticia)
        {
            return !string.IsNullOrWhiteSpace(noticia.Titulo) &&
                   noticia.Titulo.Length >= 10 &&
                   !string.IsNullOrWhiteSpace(noticia.Contenido) &&
                   noticia.Contenido.Length >= 10 &&
                   !EsContenidoExcluido(noticia.Titulo) &&
                   !EsContenidoExcluido(noticia.Contenido) &&
                   noticia.Titulo.Split(' ').Length >= 2; // Al menos 2 palabras
        }

        // CONTENIDO QUE DEBEMOS EXCLUIR (UNIVERSAL)
        private bool EsContenidoExcluido(string texto)
        {
            var textoLower = texto.ToLowerInvariant();

            var exclusiones = new[]
            {
        "cookie", "política de privacidad", "aviso legal", "términos",
        "suscríbete", "newsletter", "mostrar más", "leer más",
        "sin título", "sin contenido", "error", "javascript",
        "menu", "navegación", "sidebar", "footer", "header",
        "publicidad", "anuncio", "patrocinado", "promoción"
    };

            return exclusiones.Any(exclusion => textoLower.Contains(exclusion));
        }

        // LIMPIEZA DE TEXTO UNIVERSAL
        private string LimpiarTextoUniversal(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return "";

            texto = texto.Trim()
                         .Replace("\n", " ")
                         .Replace("\t", " ")
                         .Replace("\r", "")
                         .Replace("  ", " ");

            // Remover patrones comunes de ruido
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"^\s*[\|\-\•]\s*", "");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\s+", " ");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"^(Leer más|Ver más|Continúa|Seguir leyendo).*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            return texto.Trim();
        }

        // CONSTRUIR URL COMPLETA UNIVERSAL
        private string ConstruirUrlCompleta(string? enlace, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(enlace)) return baseUrl;

            if (enlace.StartsWith("http")) return enlace;
            if (enlace.StartsWith("//")) return "https:" + enlace;
            if (enlace.StartsWith("/"))
            {
                var base_url = GetBaseUrl(baseUrl);
                return base_url + enlace;
            }

            return baseUrl;
        }

        // OBTENER URL BASE DE CUALQUIER URL
        private string GetBaseUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return $"{uri.Scheme}://{uri.Host}";
            }
            catch
            {
                return url;
            }
        }

        // ESTRATEGIA 3: HTTP + REGEX UNIVERSAL (FALLBACK)
        private async Task<List<NoticiaExtraida>> ExtraerConHttpUniversal(string url, int limite)
        {
            var noticias = new List<NoticiaExtraida>();

            try
            {
                logger.LogInformation($"🌐 Iniciando HTTP Universal para: {url}");

                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode) return noticias;

                var html = await response.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(html)) return noticias;

                // PATRONES REGEX UNIVERSALES PARA NOTICIAS
                var patronesUniversales = new[]
                {
            @"<h[1-4][^>]*><a[^>]*href=['""]([^'""]*)['""][^>]*>([^<]+)</a></h[1-4]>",
            @"<a[^>]*href=['""]([^'""]*)['""][^>]*class=['""][^'""]*(?:title|headline|news)[^'""]*['""][^>]*>([^<]+)</a>",
            @"<article[^>]*>[\s\S]*?<h[1-4][^>]*>([^<]+)</h[1-4]>[\s\S]*?</article>",
            @"<div[^>]*class=['""][^'""]*(?:news|article|story)[^'""]*['""][^>]*>[\s\S]*?<h[1-4][^>]*>([^<]+)</h[1-4]>[\s\S]*?</div>"
        };

                foreach (var patron in patronesUniversales)
                {
                    var matches = System.Text.RegularExpressions.Regex.Matches(html, patron, System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                    foreach (System.Text.RegularExpressions.Match match in matches.Take(limite - noticias.Count))
                    {
                        try
                        {
                            var titulo = match.Groups.Count > 2 ? match.Groups[2].Value : match.Groups[1].Value;
                            var enlace = match.Groups.Count > 2 ? match.Groups[1].Value : "";

                            titulo = System.Text.RegularExpressions.Regex.Replace(titulo, @"<[^>]+>", "").Trim();

                            if (!string.IsNullOrWhiteSpace(titulo) && titulo.Length > 10)
                            {
                                var noticia = new NoticiaExtraida
                                {
                                    Titulo = LimpiarTextoUniversal(titulo),
                                    Contenido = titulo, // Como fallback usamos el título
                                    Url = ConstruirUrlCompleta(enlace, url),
                                    FechaPublicacion = DateTime.Now,
                                    Fuente = ExtraerNombreFuente(url),
                                    Hashtags = new List<string>()
                                };

                                if (EsNoticiaValidaUniversal(noticia))
                                {
                                    noticias.Add(noticia);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogDebug($"Error procesando match HTTP: {ex.Message}");
                        }

                        if (noticias.Count >= limite) break;
                    }

                    if (noticias.Count >= limite) break;
                }

                logger.LogInformation($"🎯 HTTP Universal extrajo {noticias.Count} noticias");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"❌ Error en HTTP Universal para {url}");
            }

            return noticias;
        }

        // USER AGENTS ALEATORIOS PARA EVITAR BLOQUEOS
        private string GetRandomUserAgent()
        {
            var userAgents = new[]
            {
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:121.0) Gecko/20100101 Firefox/121.0",
        "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36"
    };

            return userAgents[new Random().Next(userAgents.Length)];
        }

        // CLASE PARA ESTRATEGIAS DE SELECTORES
        public class SelectorStrategy
        {
            public string[] Selectores { get; set; } = Array.Empty<string>();
            public string[] TituloSelectores { get; set; } = Array.Empty<string>();
            public string[] ContenidoSelectores { get; set; } = Array.Empty<string>();
            public string[] EnlaceSelectores { get; set; } = Array.Empty<string>();
            public bool EsEnlaceDirecto { get; set; } = false;
            public string Nombre { get; set; } = "";
        }

    }
}