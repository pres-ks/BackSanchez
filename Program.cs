using ConsumoAPI2.Api.Data;
using ConsumoAPI2.Api.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core - Configuración para Desarrollo/Producción
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        options.UseNpgsql(connectionString);
    }
});

// CORS
var corsPolicy = "_allowFront";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicy, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5069",
                "https://localhost:7000",
                "https://*.up.railway.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseCors(corsPolicy);
app.UseSwagger();
app.UseSwaggerUI();

// Puerto para Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Add($"http://0.0.0.0:{port}");

// Tus endpoints
app.MapGet("/", () => "ConsumoAPI2.Api - Backend con PostgreSQL!");
app.MapGet("/api/products", async (AppDbContext db) => await db.Products.OrderBy(p => p.Id).ToListAsync());
app.MapGet("/api/products/{id:int}", async (int id, AppDbContext db) => await db.Products.FindAsync(id) is { } p ? Results.Ok(p) : Results.NotFound());
app.MapPost("/api/products", async (Product product, AppDbContext db) =>
{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);
});
app.MapPut("/api/products/{id:int}", async (int id, Product input, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    p.Name = input.Name;
    p.Price = input.Price;
    p.Stock = input.Stock;
    await db.SaveChangesAsync();
    return Results.NoContent();
});
app.MapDelete("/api/products/{id:int}", async (int id, AppDbContext db) =>
{
    var p = await db.Products.FindAsync(id);
    if (p is null) return Results.NotFound();
    db.Products.Remove(p);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();