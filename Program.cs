var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Configurar CORS básico
app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());

// Endpoints simples SIN base de datos
app.MapGet("/", () => "✅ Backend .NET 8 funcionando SIN base de datos!");
app.MapGet("/health", () => new { 
    status = "Healthy", 
    version = "8.0",
    timestamp = DateTime.UtcNow 
});
app.MapGet("/test", () => "Test exitoso");

// Configurar puerto
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

Console.WriteLine($"🚀 Aplicación .NET 8 iniciada en puerto {port}");
app.Run();
