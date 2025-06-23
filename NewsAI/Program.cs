var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAR PUERTO PARA RAILWAY =====
var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ===== SERVICIOS BÁSICOS =====
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ===== CORS PERMISIVO =====
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

// ===== MIDDLEWARE =====
app.UseCors();
app.UseRouting();
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();

// ===== RUTAS DE PRUEBA =====
app.MapGet("/", () => "🚀 NewsAI Backend funcionando correctamente!");

app.MapGet("/api/health", () => new { 
    status = "OK", 
    timestamp = DateTime.UtcNow,
    message = "Backend funcionando sin base de datos"
});

app.MapGet("/test", () => "Test OK - Sin dependencias externas");

// Ruta fallback para frontend
app.MapFallbackToFile("index.html");

Console.WriteLine($"🚀 Starting NewsAI on port {port}");
Console.WriteLine($"🌐 Environment: {app.Environment.EnvironmentName}");

app.Run();