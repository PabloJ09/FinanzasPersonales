using FinanzasPersonales.Database;
using FinanzasPersonales.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

Env.Load(); // 🔒 Cargar variables desde .env

var builder = WebApplication.CreateBuilder(args);

// ============================
// CONFIGURACIÓN DE SERVICIOS
// ============================

// MongoDBSettings
var mongoSettings = new MongoDBSettings
{
    ConnectionString = Environment.GetEnvironmentVariable("MONGO_URI")!,
    DatabaseName = Environment.GetEnvironmentVariable("MONGO_DB")!
};

// Registrar MongoDB y servicios
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDBContext>();

builder.Services.AddScoped<UsuarioService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<TransaccionService>();

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================
// PIPELINE
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ============================
// ENDPOINTS DE PRUEBA
// ============================

app.MapGet("/", () => "API de Finanzas Personales funcionando!");

// Test MongoDB
app.MapGet("/testmongo", (MongoDBContext db) =>
{
    return $"Conexión MongoDB establecida con DB: {db.DatabaseName}";
});

// WeatherForecast de ejemplo
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

// ============================
// RECORD DE PRUEBA
// ============================
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
