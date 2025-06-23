using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;
using NewsAI.Dominio.Repositorios;

namespace NewsAI.Infraestructura.Persistencia.Repositorios;

public class ConfiguracionRepository : IConfiguracionRepository
{
    private readonly AppDbContext context;
    private readonly ILogger<ConfiguracionRepository> logger;

    public ConfiguracionRepository(AppDbContext context, ILogger<ConfiguracionRepository> logger)
    {
        this.context = context;
        this.logger = logger;
    }


    public async Task<Configuracion> CrearAsync(Configuracion configuracion)
    {
        try
        {
            logger.LogInformation($"Repositorio: Creando nueva configuración para usuario {configuracion.UsuarioId}");

            // 1. VERIFICAR QUE EL USUARIO EXISTE
            var usuarioExiste = await context.Usuarios
                .AnyAsync(u => u.Id == configuracion.UsuarioId);

            if (!usuarioExiste)
            {
                throw new InvalidOperationException($"El usuario con ID {configuracion.UsuarioId} no existe");
            }

            
            configuracion.Id = 0; // Asegurar que no se asigne un ID manualmente
            configuracion.Activa = true;
            configuracion.Email = true; // Por defecto enviar por email
            configuracion.Audio = configuracion.Audio; // Mantener el valor o false por defecto(proxima interacion)
            configuracion.Frecuencia = configuracion.Frecuencia ?? "diaria";
            configuracion.GradoDesarrolloResumen = configuracion.GradoDesarrolloResumen ?? "Detallado";
            configuracion.Lenguaje = configuracion.Lenguaje ?? "Español";

            // Calcular próxima ejecución
            configuracion.ProximaEjecucion = CalcularProximaEjecucion(configuracion.Frecuencia);

            context.Configuraciones.Add(configuracion);
            await context.SaveChangesAsync(); //se asgina el id de la configuracion

            // se asigna la configuracion al usuaruio con el id anterior
            var configuracionUsuario = new Configuracion_Usuario
            {
                UsuarioId = configuracion.UsuarioId,
                ConfiguracionId = configuracion.Id
            };

            context.Configuracion_usuarios.Add(configuracionUsuario);
            await context.SaveChangesAsync();

            await context.Entry(configuracion)
                .Reference(c => c.Usuario)
                .LoadAsync();

            logger.LogInformation($"Repositorio: Configuración creada con ID {configuracion.Id}");
            return configuracion;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error en repositorio al crear configuración para usuario {configuracion.UsuarioId}");
            throw;
        }
    }
    public async Task<List<Configuracion>> ObtenerPorUsuarioIdAsync(int usuarioId)
    {
        try
        {
            logger.LogInformation($"Repositorio: Buscando configuraciones para usuario {usuarioId}");

            var configuraciones = await context.Configuraciones
                .Include(c => c.Usuario)
                .Where(c => c.UsuarioId == usuarioId)
                .OrderByDescending(c => c.Id) // Las más recientes primero
                .ToListAsync();

            logger.LogInformation($"Repositorio: Encontradas {configuraciones.Count} configuraciones");
            return configuraciones;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error en repositorio al obtener configuraciones para usuario {usuarioId}");
            throw;
        }
    }

    public async Task<Configuracion> ObtenerPorIdAsync(int id)
    {
        try
        {
            return await context.Configuraciones
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error al obtener configuración por ID {id}");
            throw;
        }
    }

    public async Task<Configuracion> GuardarAsync(Configuracion configuracion)
    {
        try
        {
            // Verificar si ya existe una configuración para este usuario
            var existente = await context.Configuraciones
                .FirstOrDefaultAsync(c => c.UsuarioId == configuracion.UsuarioId);

            if (existente != null)
            {
                // Actualizar configuración existente
                existente.Hashtags = configuracion.Hashtags;
                existente.GradoDesarrolloResumen = configuracion.GradoDesarrolloResumen;
                existente.Lenguaje = configuracion.Lenguaje;
                existente.Audio = configuracion.Audio;

                context.Configuraciones.Update(existente);
                await context.SaveChangesAsync();
                return existente;
            }
            else
            {
                // Crear nueva configuración
                context.Configuraciones.Add(configuracion);
                await context.SaveChangesAsync();
                return configuracion;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al guardar configuración");
            throw;
        }
    }

    
    public async Task<Configuracion> ActualizarAsync(Configuracion configuracion)
    {
        try
        {
            logger.LogInformation($"Repositorio: Actualizando configuración {configuracion.Id}");

            var existente = await context.Configuraciones
                .FirstOrDefaultAsync(c => c.Id == configuracion.Id);

            if (existente == null)
            {
                logger.LogWarning($"Configuración {configuracion.Id} no encontrada para actualizar");
                return null;
            }

            // Actualizar propiedades
            existente.Hashtags = configuracion.Hashtags;
            existente.GradoDesarrolloResumen = configuracion.GradoDesarrolloResumen;
            existente.Lenguaje = configuracion.Lenguaje;
            existente.Audio = configuracion.Audio;

            context.Configuraciones.Update(existente);
            await context.SaveChangesAsync();

            // Recargar con relaciones
            await context.Entry(existente)
                .Reference(c => c.Usuario)
                .LoadAsync();

            logger.LogInformation($"Repositorio: Configuración {configuracion.Id} actualizada exitosamente");
            return existente;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error en repositorio al actualizar configuración {configuracion.Id}");
            throw;
        }
    }

    public async Task<List<Configuracion>> ObtenerActivasParaEjecucionAsync()
    {
        try
        {
            var ahora = DateTime.Now; // hora local 

            return await context.Configuraciones
                .Include(c => c.Usuario)
                .Include(c => c.ConfiguracionUrls)
                    .ThenInclude(cu => cu.UrlConfiable)
                .Where(c => c.Activa &&
                           c.Frecuencia != "pausada" &&
                           c.ProximaEjecucion.HasValue &&
                           c.ProximaEjecucion <= ahora)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo configuraciones activas para ejecución");
            throw;
        }
    }

    public async Task<bool> ActualizarProximaEjecucionAsync(int configuracionId, DateTime? proximaEjecucion)
    {
        try
        {
            var configuracion = await context.Configuraciones.FindAsync(configuracionId);
            if (configuracion == null) return false;

            configuracion.ProximaEjecucion = proximaEjecucion;
            await context.SaveChangesAsync();

            logger.LogInformation($"Próxima ejecución actualizada para configuración {configuracionId}: {proximaEjecucion}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error actualizando próxima ejecución para configuración {configuracionId}");
            throw;
        }
    }
    // Método auxiliar para calcular próxima ejecución
    private DateTime CalcularProximaEjecucion(string frecuencia)
    {
        var ahora = DateTime.Now;

        return frecuencia?.ToLower() switch
        {
            "diaria" => ahora.Date.AddDays(1).Add(new TimeSpan(8, 0, 0)), // Mañana a las 8:00
            "semanal" => ahora.Date.AddDays(7).Add(new TimeSpan(8, 0, 0)), // Próxima semana a las 8:00
            "mensual" => ahora.Date.AddMonths(1).Add(new TimeSpan(8, 0, 0)), // Próximo mes a las 8:00
            "personalizada" => ahora.Date.AddDays(1).Add(new TimeSpan(8, 0, 0)), // Default mañana
            _ => ahora.Date.AddDays(1).Add(new TimeSpan(8, 0, 0)) // Default diaria
        };
    }
    public async Task<List<Configuracion>> ObtenerActivasAsync()
    {
        try
        {
            logger.LogInformation("Obteniendo todas las configuraciones activas");

            return await context.Configuraciones
                .Include(c => c.Usuario)
                .Include(c => c.ConfiguracionUrls)
                    .ThenInclude(cu => cu.UrlConfiable)
                .Where(c => c.Activa &&
                           c.SchedulingActivo &&
                           c.Frecuencia != "pausada")
                .ToListAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo configuraciones activas");
            throw;
        }
    }

    public async Task<bool> ActualizarUltimaEjecucionAsync(int configuracionId, DateTime ultimaEjecucion)
    {
        try
        {
            var configuracion = await context.Configuraciones.FindAsync(configuracionId);
            if (configuracion == null) return false;

            configuracion.UltimaEjecucion = ultimaEjecucion;
            await context.SaveChangesAsync();

            logger.LogInformation($"Última ejecución actualizada para configuración {configuracionId}: {ultimaEjecucion}");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error actualizando última ejecución para configuración {configuracionId}");
            throw;
        }
    }
    // Métodos existentes (crear, obtener, etc.) se mantienen igual...
    public async Task<List<Configuracion>> ObtenerTodasAsync()
    {
        return await context.Configuraciones
            .Include(c => c.Usuario)
            .ToListAsync();
    }

    public async Task<List<Configuracion>> ObtenerPorUsuarioAsync(int usuarioId)
    {
        return await context.Configuraciones
            .Where(c => c.UsuarioId == usuarioId)
            .ToListAsync();
    }

    public async Task<Configuracion> ObtenerConUrlsAsync(int configuracionId)
    {
        try
        {
            return await context.Configuraciones
                .Include(c => c.Usuario)
                .Include(c => c.ConfiguracionUrls)
                    .ThenInclude(cu => cu.UrlConfiable)
                .FirstOrDefaultAsync(c => c.Id == configuracionId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error obteniendo configuración {configuracionId} con URLs");
            throw;
        }
    }
}

