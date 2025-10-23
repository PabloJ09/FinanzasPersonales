using FinanzasPersonales.Database;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

Env.Load(); // Cargar variables desde .env

var builder = WebApplication.CreateBuilder(args);

// Configurar MongoDBSettings usando las variables de entorno
var mongoSettings = new MongoDBSettings
{
    ConnectionString = Environment.GetEnvironmentVariable("MONGO_URI")!,
    DatabaseName = Environment.GetEnvironmentVariable("MONGO_DB")!
};

// Registrar MongoDBContext como singleton
builder.Services.AddSingleton(mongoSettings);
builder.Services.AddSingleton<MongoDBContext>();

// Configuración de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/testmongo", (MongoDBContext db) =>
{
    return $"Conexión MongoDB establecida con DB: {db.Transacciones.Database.DatabaseNamespace.DatabaseName}";
});

// Habilitar Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// Endpoint base
app.MapGet("/", () => "🚀 API de Finanzas Personales funcionando!");

// Endpoint de prueba (weatherforecast)
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

// Record de prueba
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

