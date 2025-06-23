using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;
using NewsAI.Dominio.Repositorios;

namespace NewsAI.Infraestructura.Persistencia.Repositorios
{
    public class SimuladorRepository : ISimuladorRepository
    {
        private readonly AppDbContext context;
        private readonly ILogger<SimuladorRepository> logger;

        public SimuladorRepository(AppDbContext context, ILogger<SimuladorRepository> logger)
        {
            this.context = context;
            this.logger = logger;
        }

        public async Task<ResumenGenerado> GuardarResumenAsync(ResumenGenerado resumen)
        {
            try
            {
                logger.LogInformation($"Guardando resumen para usuario {resumen.UsuarioId}");

                context.Resumen.Add(resumen);
                await context.SaveChangesAsync();

                // Recargar con relaciones
                await context.Entry(resumen)
                    .Reference(r => r.Usuario)
                    .LoadAsync();
                await context.Entry(resumen)
                    .Reference(r => r.Configuracion)
                    .LoadAsync();

                logger.LogInformation($"Resumen guardado con ID {resumen.Id}");
                return resumen;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error guardando resumen");
                throw;
            }
        }

        public async Task<List<ResumenGenerado>> ObtenerHistorialPorUsuarioAsync(int usuarioId)
        {
            try
            {
                return await context.Resumen
                    .Include(r => r.Usuario)
                    .Include(r => r.Configuracion)
                    .Where(r => r.UsuarioId == usuarioId)
                    .OrderByDescending(r => r.FechaGeneracion)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo historial para usuario {usuarioId}");
                throw;
            }
        }

        public async Task<ResumenGenerado> ObtenerResumenPorIdAsync(int id)
        {
            try
            {
                return await context.Resumen
                    .Include(r => r.Usuario)
                    .Include(r => r.Configuracion)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error obteniendo resumen {id}");
                throw;
            }
        }

        public async Task<bool> ActualizarEstadoEmailAsync(int resumenId, bool emailEnviado)
        {
            try
            {
                var resumen = await context.Resumen.FindAsync(resumenId);
                if (resumen == null) return false;

                resumen.EmailEnviado = emailEnviado;
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error actualizando estado email para resumen {resumenId}");
                throw;
            }
        }

        public async Task<bool> EliminarResumenAsync(int id)
        {
            try
            {
                var resumen = await context.Resumen.FindAsync(id);
                if (resumen == null) return false;

                context.Resumen.Remove(resumen);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error eliminando resumen {id}");
                throw;
            }
        }
    }
}