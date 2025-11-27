var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Endpoints básicos de prueba
app.MapGet("/", () => "✅ Backend funcionando - " + DateTime.UtcNow);
app.MapGet("/test", () => new { status = "OK", time = DateTime.UtcNow });
app.MapGet("/health", () => "Healthy");

// Configuración del puerto
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Iniciando en puerto: {port}");
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine("🎉 APLICACIÓN INICIADA CORRECTAMENTE");
app.Run();
