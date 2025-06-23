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

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(ConfiguracionProfile));
builder.Services.AddAutoMapper(typeof(UsuarioProfile));

var openAIKey = builder.Configuration["OpenAI:ApiKey"];
var modelOpenAI = builder.Configuration["OpenAI:Model"];
var kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion(modelOpenAI, openAIKey)
    .Build();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=newsAI.db", sqLiteOptions =>
    {
        sqLiteOptions.CommandTimeout(30); // Timeout más largo
    }));

builder.Services.AddSingleton(kernel);

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

// Registrar el plugin
kernel.ImportPluginFromType<SummarizePlugin>();

// Configurar CORS para Astro
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAstro", policy =>
    {
        policy.WithOrigins(
                "http://localhost:4321",
                "https://localhost:4321",
                "http://127.0.0.1:4321",
                "https://127.0.0.1:4321"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400))
            .WithExposedHeaders("*")
            .SetIsOriginAllowed(origin => true); // MUY permisivo para debugging
    });
});
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = "Data Source=newsAI.db";
    
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



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// IMPORTANTE: CORS debe ir antes de UseAuthorization
app.UseCors("AllowAstro");


app.UseAuthorization();

app.MapControllers();

app.Run();
