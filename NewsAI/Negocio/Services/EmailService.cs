using System.Net.Mail;
using System.Net;
using NewsAI.Negocio.Interfaces;
using NewsAI.Dominio.Entidades;
using System.Text;
using System.Net.Mime;

namespace NewsAI.Negocio.Servicios
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<EmailService> logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        public async Task<bool> EnviarResumenPorEmailAsync(ResumenGenerado resumen, Configuracion configuracion)
        {
            try
            {
                logger.LogInformation($"Enviando resumen por email para usuario {resumen.UsuarioId}");

                var smtpClient = ConfigurarSmtpClient();
                var mailMessage = ConstruirEmailResumen(resumen, configuracion);

                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInformation("Email de resumen enviado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando email de resumen");
                return false;
            }
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        public async Task<bool> EnviarEmailTestAsync(string destinatario, string asunto, string contenido)
        {
            try
            {
                var smtpClient = ConfigurarSmtpClient();
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(
                        configuration["Email:FromAddress"] ?? "noreply@noticiasia.com",
                        "Noticias IA - Simulador"
                    ),
                    Subject = asunto,
                    Body = contenido,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(destinatario);
                await smtpClient.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error enviando email de prueba");
                return false;
            }
        }

        // 🚀 MÉTODO REQUERIDO POR LA INTERFAZ - SIMULADOR
        public async Task<bool> EnviarResumenAsync(string emailDestino, string contenidoResumen, Configuracion configuracion, int noticiasRelevantes)
        {
            try
            {
                logger.LogInformation("🎯 Enviando email simulador a: {EmailDestino}", emailDestino);
                logger.LogInformation("📊 Contenido: {Length} chars, Noticias: {Count}", contenidoResumen.Length, noticiasRelevantes);

                var smtpClient = ConfigurarSmtpClient();
                var mailMessage = ConstruirEmailSimulador(emailDestino, contenidoResumen, configuracion, noticiasRelevantes);

                await smtpClient.SendMailAsync(mailMessage);
                logger.LogInformation(" Email simulador enviado exitosamente");

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " Error enviando email simulador a {EmailDestino}", emailDestino);
                return false;
            }
        }

        // 🚀 MÉTODO MEJORADO ADICIONAL - PARA SCHEDULINGSERVICE
        public async Task EnviarResumenAsync(string destinatario, string resumen, List<string> hashtags)
        {
            try
            {
                logger.LogInformation("📧 Iniciando envío de email a: {Destinatario}", destinatario);
                logger.LogInformation("📊 Contenido a enviar: {Length} caracteres", resumen.Length);

                using var message = new MailMessage();

                // 🎯 CONFIGURACIÓN BÁSICA - USANDO TU CONFIGURACIÓN
                var fromEmail = configuration["Email:FromAddress"] ?? "noreply@noticiasia.com";
                var fromName = configuration["Email:FromName"] ?? "NewsAI - Resumen Personalizado";

                message.From = new MailAddress(fromEmail, fromName);
                message.To.Add(destinatario);
                message.Subject = $"📰 Tu Resumen NewsAI - {DateTime.Now:dd/MM/yyyy HH:mm}";

                // 🎨 CREAR CONTENIDO HTML COMPLETO
                var htmlBody = CrearContenidoHtml(resumen, hashtags);
                var textBody = CrearContenidoTexto(resumen, hashtags);

                //  CONFIGURAR MENSAJE MULTIPART
                message.IsBodyHtml = true;
                message.Body = htmlBody; // HTML como principal

                // 📎 AGREGAR VERSIÓN TEXTO PLANO ALTERNATIVA
                var textView = AlternateView.CreateAlternateViewFromString(textBody, Encoding.UTF8, MediaTypeNames.Text.Plain);
                var htmlView = AlternateView.CreateAlternateViewFromString(htmlBody, Encoding.UTF8, MediaTypeNames.Text.Html);

                message.AlternateViews.Add(textView);
                message.AlternateViews.Add(htmlView);

                // 🔧 HEADERS PARA EVITAR SPAM
                message.Headers.Add("X-Mailer", "NewsAI-System");
                message.Headers.Add("X-Priority", "3"); // Normal priority
                message.Priority = MailPriority.Normal;

                // 📤 ENVIAR EMAIL - USANDO TU CONFIGURACIÓN SMTP
                var smtpClient = ConfigurarSmtpClient();
                smtpClient.Timeout = 30000; // 30 segundos timeout

                await smtpClient.SendMailAsync(message);

                logger.LogInformation(" Email enviado exitosamente. HTML: {HtmlLength} chars, Texto: {TextLength} chars",
                    htmlBody.Length, textBody.Length);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, " Error enviando email a {Destinatario}", destinatario);
                throw;
            }
        }

        // 🎨 MÉTODO AUXILIAR: CREAR HTML COMPLETO
        private string CrearContenidoHtml(string resumen, List<string> hashtags)
        {
            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>NewsAI - Resumen Personalizado</title>
    <style>
        body {{ 
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; 
            line-height: 1.6; 
            color: #333; 
            max-width: 800px; 
            margin: 0 auto; 
            padding: 20px;
            background-color: #f8f9fa;
        }}
        .header {{ 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); 
            color: white; 
            padding: 30px; 
            border-radius: 10px; 
            text-align: center; 
            margin-bottom: 30px;
        }}
        .header h1 {{ 
            margin: 0; 
            font-size: 28px; 
            font-weight: 300;
        }}
        .header p {{ 
            margin: 5px 0 0 0; 
            opacity: 0.9; 
            font-size: 14px;
        }}
        .hashtags {{ 
            background: #e8f4fd; 
            padding: 15px; 
            border-radius: 8px; 
            margin-bottom: 25px;
            border-left: 4px solid #2196F3;
        }}
        .hashtags h3 {{ 
            margin: 0 0 10px 0; 
            color: #1976D2; 
            font-size: 16px;
        }}
        .hashtag {{ 
            display: inline-block; 
            background: #2196F3; 
            color: white; 
            padding: 4px 12px; 
            border-radius: 20px; 
            margin: 2px; 
            font-size: 12px;
            font-weight: 500;
        }}
        .content {{ 
            background: white; 
            padding: 30px; 
            border-radius: 10px; 
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            margin-bottom: 20px;
        }}
        .content h2 {{ 
            color: #2c3e50; 
            border-bottom: 2px solid #3498db; 
            padding-bottom: 10px; 
            margin-bottom: 20px;
        }}
        .content p {{ 
            margin-bottom: 15px; 
            text-align: justify;
        }}
        .footer {{ 
            text-align: center; 
            font-size: 12px; 
            color: #666; 
            margin-top: 30px;
            padding: 20px;
            background: white;
            border-radius: 8px;
        }}
        .highlight {{ 
            background: #fff3cd; 
            padding: 2px 4px; 
            border-radius: 3px;
        }}
        
        /* 📱 RESPONSIVE */
        @media only screen and (max-width: 600px) {{
            body {{ padding: 10px; }}
            .header {{ padding: 20px; }}
            .header h1 {{ font-size: 24px; }}
            .content {{ padding: 20px; }}
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>📰 NewsAI</h1>
        <p>Tu resumen personalizado de noticias • {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    </div>
    
    <div class='hashtags'>
        <h3>🏷️ Filtros aplicados:</h3>
        {string.Join("", hashtags.Select(tag => $"<span class='hashtag'>#{tag}</span>"))}
    </div>
    
    <div class='content'>
        <h2>📋 Resumen de Noticias</h2>
        {FormatearResumenParaHtml(resumen)}
    </div>
    
    <div class='footer'>
        <p>🤖 Generado automáticamente por <strong>NewsAI</strong></p>
        <p>Sistema inteligente de agregación y resumen de noticias</p>
        <p style='margin-top: 15px; font-size: 11px; color: #999;'>
            Este email contiene {resumen.Length:N0} caracteres de contenido
        </p>
    </div>
</body>
</html>";

            return html;
        }

        // 📝 MÉTODO AUXILIAR: CREAR TEXTO PLANO
        private string CrearContenidoTexto(string resumen, List<string> hashtags)
        {
            var texto = $@"
📰 NEWSAI - RESUMEN PERSONALIZADO
{new string('=', 50)}

🕒 Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}

🏷️ FILTROS APLICADOS:
{string.Join(", ", hashtags.Select(tag => $"#{tag}"))}

📋 RESUMEN DE NOTICIAS:
{resumen}

{new string('-', 50)}
🤖 Generado automáticamente por NewsAI
Sistema inteligente de agregación y resumen de noticias

📊 Estadísticas: {resumen.Length:N0} caracteres de contenido
";

            return texto.Trim();
        }

        // 🎨 MÉTODO AUXILIAR: FORMATEAR RESUMEN PARA HTML
        private string FormatearResumenParaHtml(string resumen)
        {
            if (string.IsNullOrEmpty(resumen))
                return "<p><em>No hay contenido disponible</em></p>";

            // Convertir saltos de línea a párrafos HTML
            var parrafos = resumen.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);

            var html = "";
            foreach (var parrafo in parrafos)
            {
                var parrafoLimpio = parrafo.Trim().Replace("\n", "<br>");

                // Resaltar números y elementos importantes
                parrafoLimpio = System.Text.RegularExpressions.Regex.Replace(
                    parrafoLimpio,
                    @"\b(\d+[%°]?|\d+:\d+)\b",
                    "<span class='highlight'>$1</span>"
                );

                html += $"<p>{parrafoLimpio}</p>\n";
            }

            return html;
        }

        //  MÉTODO ORIGINAL - TU CONFIGURACIÓN SMTP
        private SmtpClient ConfigurarSmtpClient()
        {
            return new SmtpClient(configuration["Email:SmtpServer"] ?? "smtp.gmail.com")
            {
                Port = int.Parse(configuration["Email:Port"] ?? "587"),
                Credentials = new NetworkCredential(
                    configuration["Email:Username"],
                    configuration["Email:Password"]
                ),
                EnableSsl = bool.Parse(configuration["Email:EnableSsl"] ?? "true"),
            };
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        private MailMessage ConstruirEmailSimulador(string emailDestino, string contenidoResumen, Configuracion configuracion, int noticiasRelevantes)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    configuration["Email:FromAddress"] ?? "noreply@noticiasia.com",
                    "Noticias IA - Simulador"
                ),
                Subject = $"🎯 Simulador NewsAI - Resumen Personalizado ({noticiasRelevantes} noticias)",
                Body = ConstruirContenidoEmailSimulador(contenidoResumen, configuracion, noticiasRelevantes),
                IsBodyHtml = true
            };

            // Usar email específico o el de configuración demo
            var destino = !string.IsNullOrEmpty(emailDestino) ? emailDestino : configuration["Email:DestinatarioDemo"];
            mailMessage.To.Add(destino ?? "demo@noticiasia.com");

            return mailMessage;
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        private MailMessage ConstruirEmailResumen(ResumenGenerado resumen, Configuracion configuracion)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(
                    configuration["Email:FromAddress"] ?? "noreply@noticiasia.com",
                    "Noticias IA"
                ),
                Subject = $"📰 Resumen Personalizado de Noticias - {resumen.FechaGeneracion:dd/MM/yyyy HH:mm}",
                Body = ConstruirContenidoEmail(resumen, configuracion),
                IsBodyHtml = true
            };

            // Por ahora usar email de desarrollo - después puedes obtener el email del usuario
            var emailDestino = configuration["Email:DestinatarioDemo"] ?? "demo@noticiasia.com";
            mailMessage.To.Add(emailDestino);

            return mailMessage;
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        private string ConstruirContenidoEmailSimulador(string contenidoResumen, Configuracion configuracion, int noticiasRelevantes)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Simulador NewsAI - Resumen</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .header p {{ margin: 10px 0 0 0; opacity: 0.9; }}
        .beta-badge {{ background: #fbbf24; color: #92400e; padding: 5px 15px; border-radius: 20px; font-size: 12px; font-weight: bold; margin: 15px 0; display: inline-block; }}
        .config-section {{ background: #f8f9fa; padding: 20px; border-left: 4px solid #4f46e5; margin: 20px 0; border-radius: 0 5px 5px 0; }}
        .config-section h3 {{ margin: 0 0 15px 0; color: #4f46e5; }}
        .config-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }}
        .config-item {{ display: flex; justify-content: space-between; }}
        .config-label {{ font-weight: 600; color: #555; }}
        .config-value {{ color: #333; }}
        .content-section {{ background: white; padding: 30px; border: 1px solid #e9ecef; }}
        .resumen {{ background: #f8f9fa; padding: 25px; border-radius: 8px; line-height: 1.8; }}
        .stats {{ display: grid; grid-template-columns: repeat(2, 1fr); gap: 15px; margin: 20px 0; }}
        .stat-item {{ text-align: center; padding: 15px; background: #f1f3f4; border-radius: 8px; }}
        .stat-number {{ font-size: 24px; font-weight: bold; color: #4f46e5; }}
        .stat-label {{ font-size: 12px; color: #666; text-transform: uppercase; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; }}
        .footer p {{ margin: 0; color: #666; font-size: 12px; }}
        .simulador-info {{ background: #dbeafe; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .simulador-info strong {{ color: #1d4ed8; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🎯 Simulador NewsAI</h1>
        <p>Resultado de tu simulación</p>
        <div class='beta-badge'>VERSIÓN BETA</div>
    </div>

    <div class='content-section'>
        <div class='simulador-info'>
            <strong>🧪 Simulación Completada</strong><br>
            Este resumen fue generado usando nuestro sistema de 4 agentes de IA especializados para demostrar cómo funcionará el procesamiento automático de noticias.
        </div>

        <div class='config-section'>
            <h3>⚙️ Configuración Aplicada</h3>
            <div class='config-grid'>
                <div class='config-item'>
                    <span class='config-label'>Hashtags de interés:</span>
                    <span class='config-value'>{configuracion.Hashtags ?? "No especificados"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Nivel de detalle:</span>
                    <span class='config-value'>{configuracion.GradoDesarrolloResumen ?? "No especificado"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Tono del resumen:</span>
                    <span class='config-value'>{configuracion.Lenguaje ?? "No especificado"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Canal:</span>
                    <span class='config-value'>{(configuracion.Audio ? "Audio/Podcast" : "Email")}</span>
                </div>
            </div>
        </div>

        <div class='stats'>
            <div class='stat-item'>
                <div class='stat-number'>{noticiasRelevantes}</div>
                <div class='stat-label'>Noticias Relevantes</div>
            </div>
            <div class='stat-item'>
                <div class='stat-number'>{contenidoResumen.Split(' ').Length}</div>
                <div class='stat-label'>Palabras en Resumen</div>
            </div>
        </div>

        <h3 style='color: #4f46e5; border-bottom: 2px solid #4f46e5; padding-bottom: 10px;'>📰 Resumen Generado por IA</h3>
        
        <div class='resumen'>
            {contenidoResumen.Replace("\n", "<br>")}
        </div>

        <div style='background: #fef3c7; border: 1px solid #f59e0b; border-radius: 8px; padding: 15px; margin: 20px 0;'>
            <strong style='color: #d97706;'>🚀 ¿Te gusta el resultado?</strong><br>
            <span style='color: #92400e; font-size: 14px;'>Este es solo una demostración. Una vez configurado, recibirás resúmenes automáticamente según la frecuencia que elijas.</span>
        </div>
    </div>

    <div class='footer'>
        <p>🤖 Este resumen fue generado por <strong>NewsAI</strong> usando:</p>
        <p>📊 Agente Clasificador • 🔍 Agente Filtrador • 📝 Agente Resumidor</p>
        <p>🧪 Simulador Beta • {DateTime.Now.Year} NewsAI</p>
    </div>
</body>
</html>";
        }

        //  MÉTODO ORIGINAL - SIN CAMBIOS
        private string ConstruirContenidoEmail(ResumenGenerado resumen, Configuracion configuracion)
        {
            return $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Resumen de Noticias</title>
    <style>
        body {{ font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .header h1 {{ margin: 0; font-size: 24px; }}
        .header p {{ margin: 10px 0 0 0; opacity: 0.9; }}
        .config-section {{ background: #f8f9fa; padding: 20px; border-left: 4px solid #667eea; margin: 20px 0; border-radius: 0 5px 5px 0; }}
        .config-section h3 {{ margin: 0 0 15px 0; color: #667eea; }}
        .config-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }}
        .config-item {{ display: flex; justify-content: space-between; }}
        .config-label {{ font-weight: 600; color: #555; }}
        .config-value {{ color: #333; }}
        .content-section {{ background: white; padding: 30px; border: 1px solid #e9ecef; }}
        .resumen {{ background: #f8f9fa; padding: 25px; border-radius: 8px; line-height: 1.8; }}
        .stats {{ display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px; margin: 20px 0; }}
        .stat-item {{ text-align: center; padding: 15px; background: #f1f3f4; border-radius: 8px; }}
        .stat-number {{ font-size: 24px; font-weight: bold; color: #667eea; }}
        .stat-label {{ font-size: 12px; color: #666; text-transform: uppercase; }}
        .footer {{ background: #f8f9fa; padding: 20px; text-align: center; border-radius: 0 0 10px 10px; }}
        .footer p {{ margin: 0; color: #666; font-size: 12px; }}
        .url-info {{ background: #e3f2fd; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .url-info strong {{ color: #1976d2; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>🤖 Resumen Personalizado de Noticias</h1>
        <p>Generado el {resumen.FechaGeneracion:dd/MM/yyyy} a las {resumen.FechaGeneracion:HH:mm}</p>
    </div>

    <div class='content-section'>
        <div class='config-section'>
            <h3>📋 Configuración Aplicada</h3>
            <div class='config-grid'>
                <div class='config-item'>
                    <span class='config-label'>Hashtags de interés:</span>
                    <span class='config-value'>{configuracion.Hashtags ?? "No especificados"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Nivel de detalle:</span>
                    <span class='config-value'>{configuracion.GradoDesarrolloResumen ?? "No especificado"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Tono del resumen:</span>
                    <span class='config-value'>{configuracion.Lenguaje ?? "No especificado"}</span>
                </div>
                <div class='config-item'>
                    <span class='config-label'>Método de entrega:</span>
                    <span class='config-value'>{(configuracion.Audio ? "Audio/Podcast" : "Email")}</span>
                </div>
            </div>
        </div>

        <div class='url-info'>
            <strong>📡 Fuente de noticias:</strong> {resumen.UrlOrigen}
        </div>

        <div class='stats'>
            <div class='stat-item'>
                <div class='stat-number'>{resumen.NoticiasProcesadas}</div>
                <div class='stat-label'>Noticias Procesadas</div>
            </div>
            <div class='stat-item'>
                <div class='stat-number'>{resumen.TiempoProcesamiento:F1}s</div>
                <div class='stat-label'>Tiempo de Procesamiento</div>
            </div>
            <div class='stat-item'>
                <div class='stat-number'>{resumen.ContenidoResumen.Split(' ').Length}</div>
                <div class='stat-label'>Palabras en Resumen</div>
            </div>
        </div>

        <h3 style='color: #667eea; border-bottom: 2px solid #667eea; padding-bottom: 10px;'>📰 Resumen Generado</h3>
        
        <div class='resumen'>
            {resumen.ContenidoResumen.Replace("\n", "<br>")}
        </div>
    </div>

    <div class='footer'>
        <p>✨ Este resumen fue generado automáticamente por <strong>Noticias IA</strong> usando inteligencia artificial</p>
        <p>🚀 Versión Beta del Simulador • {DateTime.Now.Year} Noticias IA</p>
    </div>
</body>
</html>";
        }
    }
}