using Microsoft.SemanticKernel;
using NewsAI.Skils;
using NewsAI.Negocio.Services;
using Microsoft.EntityFrameworkCore;
using NewsAI.API.Mappers;
using NewsAI.Negocio.Interfaces;
using NewsAI.Dominio.Repositorios;
using NewsAI.Infraestructura.Persistencia.Repositorios;
using NewsAI.Negocio.Servicios;
using NewsAI.Negocio.Interfaces.Agentes;
using NewsAI.Negocio.Services.Agentes;
using NewsAI.Negocio.Agentes;

var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAR PUERTO PARA RAILWAY =====
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(ConfiguracionProfile));
builder.Services.AddAutoMapper(typeof(UsuarioProfile));

// ===== SEMANTIC KERNEL CON PROTECCIÓN =====
var openAIKey = Environment.GetEnvironmentVariable("SEMANTIC_KERNEL_API_KEY") 
    ?? builder.Configuration["OpenAI:ApiKey"] 
    ?? "dummy-key"; // Para evitar crash
var modelOpenAI = builder.Configuration["OpenAI:Model"] ?? "gpt-4";

Kernel? kernel = null;
try 
{
    if (openAIKey != "dummy-key")
    {
        kernel = Kernel.CreateBuilder()
            .AddOpenAIChatCompletion(modelOpenAI, openAIKey)
            .Build();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not initialize Semantic Kernel: {ex.Message}");
}

// ===== BASE DE DATOS (SOLO UNA VEZ) =====
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? "Data Source=/tmp/newsAI.db";
    options.UseSqlite(connectionString, sqliteOptions =>
    {
        sqliteOptions.CommandTimeout(30);
    });
    
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ===== SERVICIOS =====
if (kernel != null)
{
    builder.Services.AddSingleton(kernel);
    // Registrar el plugin solo si kernel existe
    kernel.ImportPluginFromType<SummarizePlugin>();
}

builder.Services.AddHttpClient<NoticiasExtractorService>();
builder.Services.AddScoped<ISimuladorService, SimuladorService>();
builder.Services.AddScoped<ISimuladorRepository, SimuladorRepository>();
builder.Services.AddScoped<INoticiasExtractorService, NoticiasExtractorService>();
builder.Services.AddScoped<ISemanticKernelService, SemanticKernelService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IConfiguracionService, ConfiguracionService>();
builder.Services.AddScoped<IConfiguracionRepository, ConfiguracionRepository>();
builder.Services.AddScoped<RssReaderService>();
builder.Services.AddScoped<IUrlConfiableRepository, UrlConfiableRepository>();
builder.Services.AddScoped<IEjecucionProgramadaRepository, EjecucionProgramadaRepository>();

// Agentes de IA
builder.Services.AddScoped<IAgenteClasificador, AgenteClasificador>();
builder.Services.AddScoped<IAgenteFiltrador, AgenteFiltrador>();
builder.Services.AddScoped<IAgenteResumidor, AgenteResumidor>();
builder.Services.AddScoped<IAgenteCoordinador, AgenteCoordinador>();

// Servicio de programación automática
builder.Services.AddHostedService<SchedulingService>();

// Servicio de base de conocimiento
builder.Services.AddScoped<IConocimientoBaseService, ConocimientoBaseService>();

// ===== CORS PERMISIVO PARA RAILWAY =====
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== CREAR BASE DE DATOS AUTOMÁTICAMENTE =====
try 
{
    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Database.EnsureCreated();
        Console.WriteLine("Database initialized successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Could not initialize database: {ex.Message}");
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===== MIDDLEWARE EN ORDEN CORRECTO =====
app.UseCors();
app.UseRouting();
app.UseStaticFiles(); // Para servir archivos del frontend
app.UseAuthorization();
app.MapControllers();

// ===== RUTAS DE PRUEBA =====
app.MapGet("/", () => "NewsAI Backend está funcionando!");
app.MapGet("/api/health", () => new { status = "OK", timestamp = DateTime.UtcNow });

// Ruta fallback para la PWA (solo si hay archivos)
app.MapFallbackToFile("index.html");

Console.WriteLine($"Starting NewsAI on port {port}");
app.Run();