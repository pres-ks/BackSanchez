using ConsumoAPI2.Api.Data;
using ConsumoAPI2.Api.Models;
using Microsoft.EntityFrameworkCore;

try
{
    Console.WriteLine("🚀 INICIANDO APLICACIÓN...");
    
    var builder = WebApplication.CreateBuilder(args);

    // LOG DETALLADO
    Console.WriteLine("📝 Configurando servicios...");
    
    // 1. PRIMERO VERIFICAR LA CONNECTION STRING
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"🔗 Connection String: {connectionString}");
    
    if (string.IsNullOrEmpty(connectionString))
    {
        Console.WriteLine("❌ ERROR: Connection string está vacía o nula");
        throw new Exception("Connection string no encontrada");
    }

    // 2. CONFIGURAR DB CONTEXT CON MANEJO DE ERRORES
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        try
        {
            Console.WriteLine($"🌍 Environment: {builder.Environment.EnvironmentName}");
            
            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("🛠️ Usando SQL Server para desarrollo");
                options.UseSqlServer(connectionString);
            }
            else
            {
                Console.WriteLine("🐘 Usando PostgreSQL para producción");
                options.UseNpgsql(connectionString);
            }
            Console.WriteLine("✅ DbContext configurado exitosamente");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ERROR configurando DbContext: {ex.Message}");
            Console.WriteLine($"📄 StackTrace: {ex.StackTrace}");
            throw;
        }
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    Console.WriteLine("🔨 Construyendo aplicación...");
    var app = builder.Build();

    Console.WriteLine("🌐 Configurando middleware...");
    app.UseSwagger();
    app.UseSwaggerUI();

    // ENDPOINTS SIMPLES PRIMERO
    app.MapGet("/", () => {
        Console.WriteLine("✅ Endpoint / ejecutado");
        return "ConsumoAPI2.Api - Backend funcionando!";
    });
    
    app.MapGet("/health", () => {
        Console.WriteLine("✅ Endpoint /health ejecutado");
        return new { status = "Healthy", timestamp = DateTime.UtcNow };
    });

    // ENDPOINTS CON DB (TEMPORALMENTE COMENTADOS)
    /*
    app.MapGet("/api/products", async (AppDbContext db) => await db.Products.OrderBy(p => p.Id).ToListAsync());
    app.MapGet("/api/products/{id:int}", async (int id, AppDbContext db) => await db.Products.FindAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());
    */

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    Console.WriteLine($"🔊 Iniciando en puerto: {port}");
    app.Urls.Add($"http://0.0.0.0:{port}");

    Console.WriteLine("🎉 APLICACIÓN INICIADA EXITOSAMENTE");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"💥 ERROR CRÍTICO: {ex.Message}");
    Console.WriteLine($"📄 StackTrace: {ex.StackTrace}");
    
    // Mantener el proceso vivo para ver los logs
    Console.WriteLine("⏳ Manteniendo proceso vivo por 5 minutos...");
    Thread.Sleep(300000); // 5 minutos
    throw;
}
