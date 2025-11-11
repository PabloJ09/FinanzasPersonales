using FinanzasPersonales.Database;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Services;
using FinanzasPersonales.Validators;
using FinanzasPersonales.Middleware;
using FinanzasPersonales.Models;
using DotNetEnv;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
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

// 🔹 Registrar Unit of Work (Patrón Coordinator)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// 🔹 Registrar Repositorios Genéricos (Principio: DRY y Dependency Inversion)
builder.Services.AddScoped(sp =>
{
    var context = sp.GetRequiredService<IMongoDBContext>();
    return new MongoRepository<Categoria>(context.Categorias);
});
builder.Services.AddScoped(sp =>
{
    var context = sp.GetRequiredService<IMongoDBContext>();
    return new MongoRepository<Transaccion>(context.Transacciones);
});
builder.Services.AddScoped(sp =>
{
    var context = sp.GetRequiredService<IMongoDBContext>();
    return new MongoRepository<Usuario>(context.Usuarios);
});

builder.Services.AddScoped<IRepository<Categoria>>(sp => sp.GetRequiredService<MongoRepository<Categoria>>());
builder.Services.AddScoped<IRepository<Transaccion>>(sp => sp.GetRequiredService<MongoRepository<Transaccion>>());
builder.Services.AddScoped<IRepository<Usuario>>(sp => sp.GetRequiredService<MongoRepository<Usuario>>());

// 🔹 Registrar Validadores (Principio: Single Responsibility)
builder.Services.AddScoped<IValidator<Categoria>, CategoriaValidator>();
builder.Services.AddScoped<IValidator<Transaccion>, TransaccionValidator>();
builder.Services.AddScoped<IValidator<Usuario>, UsuarioValidator>();

// 🔹 Registrar servicios de dominio con sus interfaces (Principio: Dependency Inversion)
builder.Services.AddScoped<ICategoriaService, CategoriaService>();
builder.Services.AddScoped<ITransaccionService, TransaccionService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();

// 🔹 Registrar controladores REST
builder.Services.AddControllers();

// 🔹 Agregar Swagger / OpenAPI con soporte JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FinanzasPersonales API",
        Version = "v1",
        Description = "API RESTful protegida con JWT para Finanzas Personales",
        Contact = new OpenApiContact
        {
            Name = "API Support",
            Email = "support@finanzaspersonales.com"
        }
    });

    // 🔒 Configuración de seguridad JWT para Swagger
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Introduce tu token JWT en el formato: **Bearer {tu_token}**",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    // Agregar XML comments si existen
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

// 🔹 Configurar autenticación JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"] ?? string.Empty;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("MongoIndexSetup");

    var dbContext = services.GetRequiredService<IMongoDBContext>();
    try
    {
        // Llamamos al setup de índices y esperamos que termine antes de exponer endpoints
        await MongoIndexSetup.CreateIndexesAsync(dbContext, logger);
        logger.LogInformation("Setup de índices finalizado correctamente.");
    }
    catch (System.Exception ex)
    {
        logger.LogError(ex, "Error al crear índices de MongoDB en inicio de la app.");
        // Dependiendo de tu política: rethrow para detener el arranque, o solo log y continuar.
        // throw;
    }
}

// ============================
// CONFIGURACIÓN DEL PIPELINE
// ============================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 🔹 Middleware de manejo centralizado de excepciones (Principio: Single Responsibility)
app.UseGlobalExceptionHandler();

// Autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// 🔒 Proteger Swagger UI y JSON a menos que el usuario esté autenticado
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/swagger"))
    {
        // Permitir acceso libre a /api/Auth (registro y login)
        if (context.Request.Path.StartsWithSegments("/api/Auth"))
        {
            await next();
            return;
        }

        if (context.User?.Identity == null || !context.User.Identity.IsAuthenticated)
        {
            await context.ChallengeAsync();
            return;
        }
    }
    await next();
});

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
