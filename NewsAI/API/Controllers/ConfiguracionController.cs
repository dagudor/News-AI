using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;
using NewsAI.Negocio.Interfaces;

namespace NewsAI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfiguracionController : ControllerBase
{
    private readonly AppDbContext context;

    private readonly ILogger<ConfiguracionController> logger;

    public ConfiguracionController(
        AppDbContext context,
        ILogger<ConfiguracionController> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    // POST: api/configuracion/crear
    [HttpPost("crear")]
    public async Task<ActionResult<object>> CrearConfiguracion([FromBody] ConfiguracionRequest request)
    {
        try
        {
            logger.LogInformation($"Creando nueva configuración para usuario {request.UsuarioId}");

            // Validar que el usuario existe
            var usuarioExiste = await context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId);
            if (!usuarioExiste)
            {
                return BadRequest(new { success = false, message = "Usuario no encontrado" });
            }

            var configuracion = new Configuracion
            {
                UsuarioId = request.UsuarioId,
                Hashtags = request.Hashtags,
                GradoDesarrolloResumen = request.ProfundidadResumen ?? "breve",
                Lenguaje = request.TonoResumen ?? "informal",
                Audio = request.AccionResumen == "audio",
                Email = request.AccionResumen == "email",
                Activa = true,
                Frecuencia = request.Frecuencia?.ToLower() ?? "diaria",
                HoraEnvio = !string.IsNullOrEmpty(request.HoraEnvio) ? TimeSpan.Parse(request.HoraEnvio) : new TimeSpan(8, 0, 0),
                DiasPersonalizados = request.DiasPersonalizados ?? "1,2,3,4,5",
                UltimaEjecucion = null,
                ProximaEjecucion = CalcularProximaEjecucion(request.Frecuencia?.ToLower() ?? "diaria", request.HoraEnvio, request.DiasPersonalizados)
            };

            context.Configuraciones.Add(configuracion);
            await context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                message = "Configuración creada correctamente",
                data = new
                {
                    id = configuracion.Id,
                    usuarioId = configuracion.UsuarioId,
                    hashtags = configuracion.Hashtags,
                    profundidadResumen = configuracion.GradoDesarrolloResumen,
                    tonoResumen = configuracion.Lenguaje,
                    accionResumen = configuracion.Audio ? "audio" : "email",
                    frecuencia = configuracion.Frecuencia,
                    horaEnvio = configuracion.HoraEnvio.ToString(@"hh\:mm"),
                    activa = configuracion.Activa,
                    proximaEjecucion = configuracion.ProximaEjecucion,
                    nombre = $"Configuración {configuracion.Id}"
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error al crear configuración: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    [HttpGet("obtener/{usuarioId}")]
    public async Task<ActionResult<object>> ObtenerConfiguracion(int usuarioId)
    {
        try
        {
            var configuraciones = await context.Configuraciones
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            if (!configuraciones.Any())
            {
                return Ok(new { success = false, message = "No se encontraron configuraciones", data = new List<object>() });
            }

            var configuracionesDto = configuraciones.Select(config => new
            {
                id = config.Id,
                usuarioId = config.UsuarioId,
                hashtags = config.Hashtags,
                profundidadResumen = config.GradoDesarrolloResumen,
                tonoResumen = config.Lenguaje,
                accionResumen = config.Audio ? "audio" : "email",
                frecuencia = config.Frecuencia ?? "diaria",
                horaEnvio = config.HoraEnvio.ToString(@"hh\:mm"),
                diasPersonalizados = config.DiasPersonalizados ?? "1,2,3,4,5",
                proximaEjecucion = config.ProximaEjecucion,
                ultimaEjecucion = config.UltimaEjecucion,
                activa = config.Activa,
                nombre = $"Configuración {config.Id}",
                email = config.Email,
                audio = config.Audio
            }).ToList();

            return Ok(new
            {
                success = true,
                message = "Configuraciones obtenidas correctamente",
                data = configuracionesDto
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error obteniendo configuraciones: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPut("actualizar")]
    public async Task<ActionResult<object>> ActualizarConfiguracion([FromBody] ConfiguracionRequest request)
    {
        try
        {
            logger.LogInformation($"Actualizando configuración {request.Id}");
            logger.LogInformation($"Datos recibidos: Frecuencia={request.Frecuencia}, Días={request.DiasPersonalizados}, Hora={request.HoraEnvio}");

            if (request.Id <= 0)
            {
                logger.LogWarning($" ID inválido: {request.Id}");
                return BadRequest(new { success = false, message = "ID de configuración inválido" });
            }

            // verficiar si existe la configuración
            var configuracion = await context.Configuraciones.FindAsync(request.Id);
            if (configuracion == null)
            {
                logger.LogWarning($" Configuración no encontrada: {request.Id}");
                return NotFound(new { success = false, message = "Configuración no encontrada" });
            }


            configuracion.Hashtags = request.Hashtags ?? configuracion.Hashtags;
            configuracion.GradoDesarrolloResumen = request.ProfundidadResumen ?? configuracion.GradoDesarrolloResumen;
            configuracion.Lenguaje = request.TonoResumen ?? configuracion.Lenguaje;
            configuracion.Audio = request.AccionResumen == "audio";
            configuracion.Email = request.AccionResumen == "email";
            var frecuenciaNormalizada = request.Frecuencia?.ToLower() ?? "diaria";
            configuracion.Frecuencia = frecuenciaNormalizada;

            if (!string.IsNullOrEmpty(request.HoraEnvio))
            {
                try
                {
                    configuracion.HoraEnvio = TimeSpan.Parse(request.HoraEnvio);
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Hora incorrecta '{request.HoraEnvio}', usando 08:00: {ex.Message}");
                    configuracion.HoraEnvio = new TimeSpan(8, 0, 0);
                }
            }
            if (!string.IsNullOrEmpty(request.DiasPersonalizados))
            {
                // Validar que los días son números válidos del 1 al 7
                try
                {
                    var dias = request.DiasPersonalizados.Split(',')
                        .Select(d => d.Trim())
                        .Where(d => !string.IsNullOrEmpty(d))
                        .Select(d => int.Parse(d))
                        .Where(d => d >= 1 && d <= 7)
                        .ToList();

                    if (dias.Any())
                    {
                        configuracion.DiasPersonalizados = string.Join(",", dias);
                    }
                    else
                    {
                        configuracion.DiasPersonalizados = "1,2,3,4,5"; 
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning($"Días personalizados incorrectos '{request.DiasPersonalizados}', usando default: {ex.Message}");
                    configuracion.DiasPersonalizados = "1,2,3,4,5";
                }
            }

            // actualziar estado
            configuracion.Activa = frecuenciaNormalizada != "pausada";

            // reclaculamos la proxima ejecuicion
            try
            {
                configuracion.ProximaEjecucion = CalcularProximaEjecucion(
                    frecuenciaNormalizada,
                    configuracion.HoraEnvio.ToString(@"hh\:mm"),
                    configuracion.DiasPersonalizados
                );
            }
            catch (Exception ex)
            {
                logger.LogWarning($"Error calculando próxima ejecución: {ex.Message}");
                configuracion.ProximaEjecucion = DateTime.UtcNow.Date.AddDays(1).Add(configuracion.HoraEnvio);
            }

            logger.LogInformation($"Guardando configuración {request.Id}...");
            await context.SaveChangesAsync();
            logger.LogInformation($"Configuración {request.Id} actualizada correctamente");

            return Ok(new
            {
                success = true,
                message = "Configuración actualizada correctamente",
                data = new
                {
                    id = configuracion.Id,
                    hashtags = configuracion.Hashtags,
                    frecuencia = configuracion.Frecuencia,
                    horaEnvio = configuracion.HoraEnvio.ToString(@"hh\:mm"),
                    diasPersonalizados = configuracion.DiasPersonalizados,
                    activa = configuracion.Activa,
                    proximaEjecucion = configuracion.ProximaEjecucion
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Se ha producido un error actualizando la configuración {request.Id}");
            logger.LogError($"Mensaje: {ex.Message}");
            logger.LogError($"stackTrace: {ex.StackTrace}");

            return StatusCode(500, new
            {
                success = false,
                message = $"Error: {ex.Message}",
                details = ex.InnerException?.Message
            });
        }
    }

    [HttpPut("pausar/{id}")]
    public async Task<ActionResult<object>> PausarConfiguracion(int id)
    {
        try
        {
            var configuracion = await context.Configuraciones.FindAsync(id);
            if (configuracion == null)
            {
                return NotFound(new { success = false, message = "Configuración no encontrada" });
            }

            configuracion.Frecuencia = "pausada";
            configuracion.Activa = false;
            configuracion.ProximaEjecucion = null;

            await context.SaveChangesAsync();

            return Ok(new { success = true, message = "Configuración pausada" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error pausando configuración {id}: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpPut("reanudar/{id}")]
    public async Task<ActionResult<object>> ReanudarConfiguracion(int id)
    {
        try
        {
            var configuracion = await context.Configuraciones.FindAsync(id);
            if (configuracion == null)
            {
                return NotFound(new { success = false, message = "Configuración no encontrada" });
            }

            configuracion.Frecuencia = "diaria"; // Por defecto
            configuracion.Activa = true;
            configuracion.ProximaEjecucion = CalcularProximaEjecucion("diaria", configuracion.HoraEnvio.ToString(@"hh\:mm"), configuracion.DiasPersonalizados);

            await context.SaveChangesAsync();

            return Ok(new { success = true, message = "Configuración reanudada" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error reanudando configuración {id}: {ex.Message}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // MÉTODO AUXILIAR MEJORADO
    private DateTime? CalcularProximaEjecucion(string frecuencia, string horaEnvio, string diasPersonalizados)
    {
        if (frecuencia == "pausada") return null;

        var hora = !string.IsNullOrEmpty(horaEnvio) ? TimeSpan.Parse(horaEnvio) : new TimeSpan(8, 0, 0);
        var ahora = DateTime.UtcNow;

        return frecuencia?.ToLower() switch
        {
            "diaria" => ahora.Date.AddDays(1).Add(hora),
            "semanal" => ahora.Date.AddDays(7).Add(hora),
            "personalizada" => CalcularProximaEjecucionPersonalizada(diasPersonalizados, hora),
            _ => ahora.Date.AddDays(1).Add(hora)
        };
    }

    private DateTime? CalcularProximaEjecucionPersonalizada(string diasPersonalizados, TimeSpan hora)
    {
        if (string.IsNullOrEmpty(diasPersonalizados)) return DateTime.UtcNow.Date.AddDays(1).Add(hora);

        var dias = diasPersonalizados.Split(',').Select(d => int.Parse(d.Trim())).ToList();
        var ahora = DateTime.UtcNow;

        // Buscar el próximo día válido
        for (int i = 1; i <= 7; i++)
        {
            var fechaCandidata = ahora.Date.AddDays(i);
            var diaSemana = (int)fechaCandidata.DayOfWeek;
            diaSemana = diaSemana == 0 ? 7 : diaSemana; // Convertir domingo de 0 a 7

            if (dias.Contains(diaSemana))
            {
                return fechaCandidata.Add(hora);
            }
        }

        return ahora.Date.AddDays(1).Add(hora);
    }

}


// Clase para recibir la request del frontend
public class ConfiguracionRequest
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Hashtags { get; set; }
    public string ProfundidadResumen { get; set; }
    public string TonoResumen { get; set; }
    public string AccionResumen { get; set; }
    public string Frecuencia { get; set; }
    public string HoraEnvio { get; set; }
    public string DiasPersonalizados { get; set; }
}