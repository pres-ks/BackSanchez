using ConsumoAPI2.Api.Data;
using ConsumoAPI2.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configurar Entity Framework con PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString);
});

// CORS - Permitir frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5069",
                "https://localhost:7000",
                "https://*.up.railway.app",
                "http://localhost:3000"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();

// Endpoint básico
app.MapGet("/", () => "API de Productos funcionando!");

// 🔥 ENDPOINTS CRUD DE PRODUCTOS 🔥

// GET - Obtener todos los productos
app.MapGet("/api/products", async (AppDbContext db) =>
{
    try
    {
        var products = await db.Products.OrderBy(p => p.Id).ToListAsync();
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al obtener productos: {ex.Message}");
    }
});

// GET - Obtener producto por ID
app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>
{
    var product = await db.Products.FindAsync(id);
    return product != null ? Results.Ok(product) : Results.NotFound();
});

// POST - Crear nuevo producto
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    try
    {
        db.Products.Add(product);
        await db.SaveChangesAsync();
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al crear producto: {ex.Message}");
    }
});

// PUT - Actualizar producto
app.MapPut("/api/products/{id}", async (int id, Product updatedProduct, AppDbContext db) =>
{
    try
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return Results.NotFound();

        product.Name = updatedProduct.Name;
        product.Price = updatedProduct.Price;
        product.Stock = updatedProduct.Stock;

        await db.SaveChangesAsync();
        return Results.Ok(product);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al actualizar producto: {ex.Message}");
    }
});

// DELETE - Eliminar producto
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>
{
    try
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return Results.NotFound();

        db.Products.Remove(product);
        await db.SaveChangesAsync();
        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error al eliminar producto: {ex.Message}");
    }
});

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

app.Run();
