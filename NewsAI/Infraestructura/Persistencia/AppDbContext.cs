using Microsoft.EntityFrameworkCore;
using NewsAI.Dominio.Entidades;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== DbSets =====
    public DbSet<Configuracion> Configuraciones { get; set; }
    public DbSet<NewsAI.Dominio.Entidades.Usuario> Usuarios { get; set; }
    public DbSet<NewsAI.Dominio.Entidades.Configuracion_Usuario> Configuracion_usuarios { get; set; }
    public DbSet<NewsAI.Dominio.Entidades.NoticiaExtraida> Noticia { get; set; }
    public DbSet<NewsAI.Dominio.Entidades.ResumenGenerado> Resumen { get; set; }
    public DbSet<UrlConfiable> UrlsConfiables { get; set; }
    public DbSet<ConfiguracionUrl> ConfiguracionUrls { get; set; }
    public DbSet<EjecucionProgramada> EjecucionesProgramadas { get; set; }
    public DbSet<ResumenGenerado> ResumenesGenerados { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ===== CONFIGURACIÓN PARA UrlConfiable =====
        modelBuilder.Entity<UrlConfiable>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TipoFuente).HasDefaultValue("RSS");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("GETUTCDATE()");

            // Índices para optimización
            entity.HasIndex(e => new { e.UsuarioId, e.Url }).IsUnique();
            entity.HasIndex(e => e.Activa);

            // Relaciones
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== CONFIGURACIÓN PARA ConfiguracionUrl =====
        modelBuilder.Entity<ConfiguracionUrl>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("GETUTCDATE()");

            // Índice único para evitar duplicados
            entity.HasIndex(e => new { e.ConfiguracionId, e.UrlConfiableId }).IsUnique();

            // Relaciones
            entity.HasOne(e => e.Configuracion)
                  .WithMany(c => c.ConfiguracionUrls)
                  .HasForeignKey(e => e.ConfiguracionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.UrlConfiable)
                  .WithMany(u => u.ConfiguracionUrls)
                  .HasForeignKey(e => e.UrlConfiableId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== CONFIGURACIÓN PARA EjecucionProgramada =====
        modelBuilder.Entity<EjecucionProgramada>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Estado).HasDefaultValue("Pendiente");

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.FechaEjecucion);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => new { e.ConfiguracionId, e.Estado });

            // Relaciones
            entity.HasOne(e => e.Configuracion)
                  .WithMany()
                  .HasForeignKey(e => e.ConfiguracionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ResumenGenerado)
                  .WithMany()
                  .HasForeignKey(e => e.ResumenGeneradoId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // ===== CONFIGURACIÓN PARA ResumenGenerado =====
        modelBuilder.Entity<ResumenGenerado>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Relación con Usuario
            entity.HasOne(r => r.Usuario)
                  .WithMany()
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Relación con Configuración
            entity.HasOne(r => r.Configuracion)
                  .WithMany()
                  .HasForeignKey(r => r.ConfiguracionId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Configuraciones de propiedades
            entity.Property(r => r.UrlOrigen)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(r => r.ContenidoResumen)
                  .IsRequired();

            // Índices para optimizar consultas del carrusel
            entity.HasIndex(r => r.UsuarioId);
            entity.HasIndex(r => r.FechaGeneracion);
            entity.HasIndex(r => new { r.UsuarioId, r.FechaGeneracion });
        });

        // =====  CONFIGURACIÓN COMPLETA PARA Configuracion CON SCHEDULING =====
        modelBuilder.Entity<Configuracion>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Campos requeridos existentes
            entity.Property(e => e.Hashtags).IsRequired().HasMaxLength(500);
            entity.Property(e => e.GradoDesarrolloResumen).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Lenguaje).IsRequired().HasMaxLength(50);
            entity.Property(e => e.UsuarioId).IsRequired();

            // Campos con defaults existentes
            entity.Property(e => e.Email).HasDefaultValue(false);
            entity.Property(e => e.Audio).HasDefaultValue(false);
            entity.Property(e => e.Activa).HasDefaultValue(true);

            // ⭐ NUEVOS CAMPOS PARA SCHEDULING AUTOMÁTICO
            entity.Property(e => e.Frecuencia)
                  .HasDefaultValue("diaria")
                  .HasMaxLength(20)
                  .IsRequired();

            entity.Property(e => e.HoraEnvio)
                  .HasDefaultValue(new TimeSpan(8, 0, 0)); // 08:00:00 por defecto

            entity.Property(e => e.DiasPersonalizados)
                  .HasMaxLength(20)
                  .HasDefaultValue(""); // "1,3,5" para Lun,Mie,Vie

            entity.Property(e => e.SchedulingActivo)
                  .HasDefaultValue(true);

            // ProximaEjecucion y UltimaEjecucion son nullable, no necesitan defaults

            // Índices útiles existentes
            entity.HasIndex(e => e.UsuarioId);
            entity.HasIndex(e => e.Activa);

            // ⭐ NUEVOS ÍNDICES PARA SCHEDULING OPTIMIZADO
            entity.HasIndex(e => e.ProximaEjecucion);
            entity.HasIndex(e => e.SchedulingActivo);
            entity.HasIndex(e => new { e.Activa, e.SchedulingActivo, e.ProximaEjecucion })
                  .HasDatabaseName("IX_Configuracion_SchedulingQuery");

            // Índice para consultas de scheduling específicas
            entity.HasIndex(e => new { e.UsuarioId, e.Activa, e.SchedulingActivo })
                  .HasDatabaseName("IX_Configuracion_UsuarioScheduling");

            // Relación con Usuario
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== CONFIGURACIÓN PARA Usuario (si necesitas añadir algo) =====
        modelBuilder.Entity<NewsAI.Dominio.Entidades.Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Asegurarse de que Email existe y es único
            entity.Property(e => e.Email)
                  .IsRequired()
                  .HasMaxLength(255);

            entity.HasIndex(e => e.Email)
                  .IsUnique();

            entity.Property(e => e.Nombre)
                  .HasMaxLength(100);
        });

        // ===== CONFIGURACIÓN PARA NoticiaExtraida =====
        modelBuilder.Entity<NewsAI.Dominio.Entidades.NoticiaExtraida>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Titulo)
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(e => e.Url)
                  .HasMaxLength(2000);

            entity.Property(e => e.Fuente)
                  .HasMaxLength(200);

            // Índices para optimización
            entity.HasIndex(e => e.FechaPublicacion);
            entity.HasIndex(e => e.Fuente);
        });

    
    

        base.OnModelCreating(modelBuilder);
    }
}