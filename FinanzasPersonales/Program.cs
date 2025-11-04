using FinanzasPersonales.Database;
using FinanzasPersonales.Services;
using DotNetEnv;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

// ============================
// CARGA DE VARIABLES DE ENTORNO
// ============================
Env.Load(); // Cargar variables desde .env

var builder = WebApplication.CreateBuilder(args);

// ============================
// CONFIGURACIÓN DE SERVICIOS
// ============================

// 🔹 Crear objeto MongoDBSettings a partir de las variables de entorno
var mongoSettings = new MongoDBSettings
{
    ConnectionString = Environment.GetEnvironmentVariable("MONGO_URI")!,
    DatabaseName = Environment.GetEnvironmentVariable("MONGO_DB")!
};

// 🔹 Registrar MongoDBSettings para inyección
builder.Services.AddSingleton(mongoSettings);

// 🔹 Registrar el contexto de base de datos mediante la interfaz
builder.Services.AddSingleton<IMongoDBContext>(sp =>
{
    var settings = sp.GetRequiredService<MongoDBSettings>();
    return new MongoDBContext(settings);
});

// 🔹 Registrar servicios de dominio
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<TransaccionService>();

// 🔹 Registrar controladores REST
builder.Services.AddControllers();

// 🔹 Agregar Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ============================
// CONFIGURACIÓN DEL PIPELINE
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// ============================
// ENDPOINTS DE PRUEBA
// ============================

// Endpoint raíz (para verificar despliegue rápido)
app.MapGet("/", () => "✅ API de Finanzas Personales funcionando correctamente!");

// Endpoint para probar conexión MongoDB
app.MapGet("/testmongo", (IMongoDBContext db) =>
{
    return $"Conexión MongoDB establecida con la base de datos: {db.DatabaseName}";
});

// Endpoint ejemplo de prueba (WeatherForecast)
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
// RECORD DE PRUEBA (solo demo)
// ============================
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
