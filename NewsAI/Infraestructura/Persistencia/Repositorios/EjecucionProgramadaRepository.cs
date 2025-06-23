using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;
using NewsAI.Dominio.Repositorios;

namespace NewsAI.Infraestructura.Persistencia.Repositorios
{
    public class EjecucionProgramadaRepository : IEjecucionProgramadaRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<EjecucionProgramadaRepository> logger;

        public EjecucionProgramadaRepository(AppDbContext context, ILogger<EjecucionProgramadaRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<EjecucionProgramada> CrearAsync(EjecucionProgramada ejecucion)
        {
            try
            {
                context.EjecucionesProgramadas.Add(ejecucion);
                await context.SaveChangesAsync();

                logger.LogInformation($"Ejecución programada creada con ID {ejecucion.Id} para configuración {ejecucion.ConfiguracionId}");
                return ejecucion;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creando ejecución programada para configuración {ejecucion.ConfiguracionId}");
                throw;
            }
        }

        public async Task<EjecucionProgramada> ActualizarAsync(EjecucionProgramada ejecucion)
        {
            try
            {
                context.EjecucionesProgramadas.Update(ejecucion);
                await context.SaveChangesAsync();
                return ejecucion;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error actualizando ejecución {ejecucion.Id}");
                throw;
            }
        }

        public async Task<List<EjecucionProgramada>> ObtenerPendientesAsync()
        {
            try
            {
                return await context.EjecucionesProgramadas
                    .Include(e => e.Configuracion)
                    .ThenInclude(c => c.Usuario)
                    .Where(e => e.Estado == "Pendiente" && e.FechaEjecucion <= DateTime.UtcNow)
                    .OrderBy(e => e.FechaEjecucion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error obteniendo ejecuciones pendientes");
                throw;
            }
        }

        public async Task<List<EjecucionProgramada>> ObtenerPorConfiguracionAsync(int configuracionId)
        {
            try
            {
                return await context.EjecucionesProgramadas
                    .Where(e => e.ConfiguracionId == configuracionId)
                    .OrderByDescending(e => e.FechaEjecucion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo ejecuciones para configuración {configuracionId}");
                throw;
            }
        }

        public async Task<EjecucionProgramada> ObtenerPorIdAsync(int id)
        {
            try
            {
                return await context.EjecucionesProgramadas
                    .Include(e => e.Configuracion)
                    .Include(e => e.ResumenGenerado)
                    .FirstOrDefaultAsync(e => e.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo ejecución {id}");
                throw;
            }
        }

        public async Task MarcarComoIniciadaAsync(int id)
        {
            try
            {
                var ejecucion = await context.EjecucionesProgramadas.FindAsync(id);
                if (ejecucion != null)
                {
                    ejecucion.Estado = "Ejecutando";
                    ejecucion.FechaInicio = DateTime.UtcNow;
                    await context.SaveChangesAsync();

                    logger.LogInformation($"Ejecución {id} marcada como iniciada");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error marcando ejecución {id} como iniciada");
                throw;
            }
        }

        public async Task MarcarComoCompletadaAsync(int id, int? resumenGeneradoId = null)
        {
            try
            {
                var ejecucion = await context.EjecucionesProgramadas.FindAsync(id);
                if (ejecucion != null)
                {
                    ejecucion.Estado = "Completada";
                    ejecucion.FechaFin = DateTime.UtcNow;
                    ejecucion.ResumenGeneradoId = resumenGeneradoId;
                    await context.SaveChangesAsync();

                    logger.LogInformation($"Ejecución {id} marcada como completada");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error marcando ejecución {id} como completada");
                throw;
            }
        }

        public async Task MarcarComoErrorAsync(int id, string mensajeError)
        {
            try
            {
                var ejecucion = await context.EjecucionesProgramadas.FindAsync(id);
                if (ejecucion != null)
                {
                    ejecucion.Estado = "Error";
                    ejecucion.FechaFin = DateTime.UtcNow;
                    ejecucion.MensajeError = mensajeError;
                    await context.SaveChangesAsync();

                    logger.LogWarning($"Ejecución {id} marcada como error: {mensajeError}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error marcando ejecución {id} como error");
                throw;
            }
        }

        public async Task<List<EjecucionProgramada>> ObtenerHistorialAsync(int usuarioId, int limite = 50)
        {
            try
            {
                return await context.EjecucionesProgramadas
                    .Include(e => e.Configuracion)
                    .Include(e => e.ResumenGenerado)
                    .Where(e => e.Configuracion.UsuarioId == usuarioId)
                    .OrderByDescending(e => e.FechaEjecucion)
                    .Take(limite)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo historial para usuario {usuarioId}");
                throw;
            }
        }
    }
}