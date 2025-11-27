using ConsumoAPI2.Api.Data;
using ConsumoAPI2.Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

// Configurar HttpClient para The Dog API
builder.Services.AddHttpClient("DogAPI", client =>
{
    client.BaseAddress = new Uri("https://api.thedogapi.com/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", "live_IO5ZjVjwigVhC3SrfgvEMNe2fB22sSL5H998b9RAtEBkXIkPfRhEDlZQuWbKAcYz");
});

// Configurar Entity Framework con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
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

// 🔥 ENDPOINTS CON THE DOG API 🔥

// GET - Obtener razas de perros desde The Dog API
app.MapGet("/api/dogs", async (IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("DogAPI");
        var response = await client.GetAsync("breeds");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Results.Ok(content);
        }
        return Results.StatusCode((int)response.StatusCode);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

// GET - Obtener una raza específica por ID
app.MapGet("/api/dogs/{id}", async (int id, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("DogAPI");
        var response = await client.GetAsync($"breeds/{id}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Results.Ok(content);
        }
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

// GET - Buscar razas por nombre
app.MapGet("/api/dogs/search/{name}", async (string name, IHttpClientFactory httpClientFactory) =>
{
    try
    {
        var client = httpClientFactory.CreateClient("DogAPI");
        var response = await client.GetAsync($"breeds/search?q={name}");
        
        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            return Results.Ok(content);
        }
        return Results.NotFound();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

// 🔥 ENDPOINTS PARA FAVORITOS (TU BASE DE DATOS) 🔥

// POST - Guardar un perro como favorito en tu DB
app.MapPost("/api/favorites", async (DogFavorite favorite, AppDbContext db) =>
{
    db.DogFavorites.Add(favorite);
    await db.SaveChangesAsync();
    return Results.Created($"/api/favorites/{favorite.Id}", favorite);
});

// GET - Obtener todos los favoritos
app.MapGet("/api/favorites", async (AppDbContext db) =>
    await db.DogFavorites.ToListAsync());

// DELETE - Eliminar de favoritos
app.MapDelete("/api/favorites/{id}", async (int id, AppDbContext db) =>
{
    var favorite = await db.DogFavorites.FindAsync(id);
    if (favorite == null) return Results.NotFound();
    
    db.DogFavorites.Remove(favorite);
    await db.SaveChangesAsync();
    return Results.Ok();
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");
app.Run();
