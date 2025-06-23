var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();

var app = builder.Build();

app.MapGet("/", () => "🚀 NewsAI funcionando!");

Console.WriteLine($"🚀 Starting NewsAI on port {port}");
app.Run();