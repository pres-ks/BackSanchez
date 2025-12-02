using ConsumoAPI2.Api.Data;
using ConsumoAPI2.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// 🔥 AGREGAR ESTO PRIMERO - MANEJO DE ERRORES EN STARTUP
try
{
    Console.WriteLine("🚀 INICIANDO APLICACIÓN...");

    // Configurar Entity Framework con PostgreSQL - CON MANEJO DE ERRORES
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    Console.WriteLine($"🔗 Connection String: {!string.IsNullOrEmpty(connectionString)}");
    
    builder.Services.AddDbContext<AppDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
    });

    // CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    });

    var app = builder.Build();
    
    app.UseCors("AllowAll");

    // 🔥 ENDPOINT RAÍZ CRÍTICO - SIEMPRE DEBE EXISTIR
    app.MapGet("/", () => "✅ API de Usuarios funcionando! Usa /api/users para gestionar usuarios.");

    // 🔥 ENDPOINT DE HEALTH CHECK
    app.MapGet("/health", () => new { 
        status = "Healthy", 
        timestamp = DateTime.UtcNow,
        database = !string.IsNullOrEmpty(connectionString)
    });

    // 🔥 ENDPOINTS PARA USUARIOS - CRUD COMPLETO
    
    // GET - Obtener todos los usuarios
    app.MapGet("/api/users", async (AppDbContext db) =>
    {
        try
        {
            return Results.Ok(await db.Users.ToListAsync());
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error obteniendo usuarios: {ex.Message}");
        }
    });

    // GET - Obtener un usuario por ID
    app.MapGet("/api/users/{id}", async (int id, AppDbContext db) =>
    {
        try
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return Results.NotFound();
            return Results.Ok(user);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error obteniendo usuario: {ex.Message}");
        }
    });

    // POST - Crear un nuevo usuario
    app.MapPost("/api/users", async (User user, AppDbContext db) =>
    {
        try
        {
            db.Users.Add(user);
            await db.SaveChangesAsync();
            return Results.Created($"/api/users/{user.Id}", user);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error creando usuario: {ex.Message}");
        }
    });

    // PUT - Actualizar un usuario existente
    app.MapPut("/api/users/{id}", async (int id, User updatedUser, AppDbContext db) =>
    {
        try
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return Results.NotFound();
            
            user.Name = updatedUser.Name;
            user.Email = updatedUser.Email;
            user.Role = updatedUser.Role;
            
            await db.SaveChangesAsync();
            return Results.Ok(user);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error actualizando usuario: {ex.Message}");
        }
    });

    // DELETE - Eliminar un usuario
    app.MapDelete("/api/users/{id}", async (int id, AppDbContext db) =>
    {
        try
        {
            var user = await db.Users.FindAsync(id);
            if (user == null) return Results.NotFound();
            
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            return Results.Ok();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error eliminando usuario: {ex.Message}");
        }
    });

    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    app.Urls.Add($"http://0.0.0.0:{port}");

    Console.WriteLine("🎉 APLICACIÓN INICIADA CORRECTAMENTE");
    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine($"💥 ERROR CRÍTICO AL INICIAR: {ex.Message}");
    Console.WriteLine($"📄 StackTrace: {ex.StackTrace}");
    throw;
}
