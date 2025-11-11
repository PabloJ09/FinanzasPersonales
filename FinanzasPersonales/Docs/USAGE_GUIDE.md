# 🔨 Guía de Uso - Arquitectura SOLID

## 📋 Tabla de Contenidos

1. [Cómo Crear una Categoría](#crear-categoría)
2. [Cómo Buscar Categorías](#buscar-categorías)
3. [Cómo Validar Datos](#validar-datos)
4. [Cómo Manejar Errores](#manejar-errores)
5. [Cómo Extender Funcionalidad](#extender-funcionalidad)

---

## 🔧 Crear Categoría

### Con la Nueva Arquitectura SOLID

```csharp
// En el Controller
[HttpPost]
public async Task<ActionResult<ApiResponse<Categoria>>> Create([FromBody] Categoria categoria)
{
    try
    {
        // ✅ El servicio maneja todo
        var creada = await _service.CreateAsync(categoria);
        
        var response = new ApiResponse<Categoria>
        {
            Success = true,
            Data = creada,
            Message = "Categoría creada exitosamente"
        };
        
        return CreatedAtAction(nameof(GetById), new { id = creada.Id }, response);
    }
    catch (ValidationException ex)
    {
        // ✅ Errores de validación capturados
        var response = new ApiResponse<Categoria>
        {
            Success = false,
            Message = ex.Message,
            Code = ex.Code,
            Errors = ex.Errors
        };
        return BadRequest(response);
    }
    catch (DomainException ex)
    {
        // ✅ Errores de negocio capturados
        return BadRequest(new ApiResponse<Categoria> 
        { 
            Success = false, 
            Message = ex.Message, 
            Code = ex.Code 
        });
    }
}

// En el Servicio
public async Task<Categoria> CreateAsync(Categoria categoria)
{
    if (categoria == null)
        throw new ArgumentNullException(nameof(categoria));

    // ✅ Validar antes de guardar
    var validationResult = await _validator.ValidateAsync(categoria);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
        throw new Common.Exceptions.ValidationException(errors);
    }

    categoria.Id = null;
    
    // ✅ Guardar usando el repositorio genérico
    return await _repository.AddAsync(categoria);
}

// En el Validador
public class CategoriaValidator : AbstractValidator<Categoria>
{
    public CategoriaValidator()
    {
        // ✅ Reglas de validación claras
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(50).WithMessage("Máximo 50 caracteres.");

        RuleFor(c => c.Tipo)
            .NotEmpty()
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("Debe ser 'Ingreso' o 'Gasto'.");

        RuleFor(c => c.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId es requerido.");
    }
}

// En el Repositorio
public class MongoRepository<T> : IRepository<T> where T : class
{
    public async Task<T> AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // ✅ MongoDB realiza la inserción
        await _collection.InsertOneAsync(entity);
        return entity;
    }
}
```

---

## 🔍 Buscar Categorías

### Opción 1: Búsqueda Simple

```csharp
// En el Servicio
public async Task<List<Categoria>> GetAllAsync()
{
    // ✅ Repositorio genérico hace todo
    return await _repository.GetAllAsync();
}

// En el Repositorio genérico
public async Task<List<T>> GetAllAsync()
{
    return await _collection.Find(_ => true).ToListAsync();
}
```

### Opción 2: Búsqueda por Predicado

```csharp
// En el Servicio
public async Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId)
{
    if (string.IsNullOrWhiteSpace(usuarioId))
        throw new ArgumentNullException(nameof(usuarioId));

    // ✅ Usa el repositorio genérico con predicado
    return await _repository.FindAsync(c => c.UsuarioId == usuarioId);
}

// En el Repositorio genérico
public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
{
    if (predicate == null)
        throw new ArgumentNullException(nameof(predicate));

    return await _collection.Find(predicate).ToListAsync();
}
```

### Opción 3: Búsqueda con Especificación (Extensible - Open/Closed)

```csharp
// Crear una especificación reutilizable
public class CategoriasPorUsuarioSpec : Specification<Categoria>
{
    public CategoriasPorUsuarioSpec(string usuarioId)
    {
        Criteria = c => c.UsuarioId == usuarioId;
        ApplyOrdering(c => c.Nombre); // Ordenar por nombre
        ApplyPaging(0, 10); // Paginar: página 1, 10 resultados
    }
}

// En el Repositorio (método que interpreta especificaciones)
public async Task<List<T>> FindWithSpecAsync(ISpecification<T> spec)
{
    var query = _collection.AsQueryable();
    
    query = query.Where(spec.Criteria);
    
    if (spec.OrderBy != null)
        query = query.OrderBy(spec.OrderBy);
    
    if (spec.OrderByDescending != null)
        query = query.OrderByDescending(spec.OrderByDescending);
    
    if (spec.IsPagingEnabled)
        query = query.Skip(spec.Skip ?? 0).Take(spec.Take ?? 10);
    
    return await query.ToListAsync();
}

// Uso en el Servicio
public async Task<List<Categoria>> GetByUsuarioIdPagedAsync(string usuarioId, int page, int pageSize)
{
    var spec = new CategoriasPorUsuarioSpec(usuarioId);
    return await _repository.FindWithSpecAsync(spec);
}
```

---

## ✅ Validar Datos

### Cómo Funciona la Validación

```csharp
// 1️⃣ Definir reglas en el Validador
public class TransaccionValidator : AbstractValidator<Transaccion>
{
    public TransaccionValidator()
    {
        RuleFor(t => t.Tipo)
            .NotEmpty().WithMessage("El tipo es obligatorio.")
            .Must(t => t == "Ingreso" || t == "Gasto");

        RuleFor(t => t.Monto)
            .NotEmpty().WithMessage("El monto es obligatorio.")
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.");

        RuleFor(t => t.CategoriaId)
            .NotEmpty().WithMessage("La categoría es requerida.")
            .Length(24).When(t => !string.IsNullOrEmpty(t.CategoriaId))
            .WithMessage("CategoriaId debe ser un ObjectId válido.");
    }
}

// 2️⃣ Usar en el Servicio
public async Task<Transaccion> CreateAsync(Transaccion transaccion)
{
    // ✅ Validar automáticamente
    var validationResult = await _validator.ValidateAsync(transaccion);
    
    if (!validationResult.IsValid)
    {
        // Convertir errores a diccionario
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key, 
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        
        // ✅ Lanzar excepción personalizada
        throw new Common.Exceptions.ValidationException(errors);
    }

    return await _repository.AddAsync(transaccion);
}

// 3️⃣ Respuesta HTTP Estandarizada
{
    "success": false,
    "data": null,
    "message": "Errores de validación",
    "code": "VALIDATION_ERROR",
    "errors": {
        "Monto": [
            "El monto es obligatorio.",
            "El monto debe ser mayor que 0."
        ],
        "CategoriaId": [
            "La categoría es requerida."
        ]
    },
    "timestamp": "2025-11-11T10:30:00Z"
}
```

---

## 🛡️ Manejar Errores

### Flujo de Manejo de Excepciones

```csharp
// 1️⃣ En el Middleware (captura TODAS las excepciones)
public class GlobalExceptionHandlerMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context); // Ejecuta el resto del pipeline
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        switch (exception)
        {
            // ✅ Entidad no encontrada → 404
            case EntityNotFoundException ex:
                context.Response.StatusCode = 404;
                return context.Response.WriteAsJsonAsync(
                    new ApiResponse { Success = false, Message = ex.Message, Code = ex.Code }
                );

            // ✅ Validación fallida → 400
            case ValidationException ex:
                context.Response.StatusCode = 400;
                return context.Response.WriteAsJsonAsync(
                    new ApiResponse { Success = false, Message = ex.Message, Code = ex.Code, Errors = ex.Errors }
                );

            // ✅ No autorizado → 401
            case UnauthorizedException ex:
                context.Response.StatusCode = 401;
                return context.Response.WriteAsJsonAsync(
                    new ApiResponse { Success = false, Message = ex.Message, Code = ex.Code }
                );

            // ✅ Error genérico de dominio → 400
            case DomainException ex:
                context.Response.StatusCode = 400;
                return context.Response.WriteAsJsonAsync(
                    new ApiResponse { Success = false, Message = ex.Message, Code = ex.Code, Errors = ex.Errors }
                );

            // ✅ Cualquier otra excepción → 500
            default:
                context.Response.StatusCode = 500;
                return context.Response.WriteAsJsonAsync(
                    new ApiResponse 
                    { 
                        Success = false, 
                        Message = "Ha ocurrido un error interno", 
                        Code = "INTERNAL_SERVER_ERROR" 
                    }
                );
        }
    }
}

// 2️⃣ El middleware se registra en Program.cs
app.UseGlobalExceptionHandler(); // Antes que todo lo demás
```

### Tipos de Excepciones Disponibles

```csharp
// EntityNotFoundException - Entidad no existe
throw new EntityNotFoundException(nameof(Categoria), categoriaId);
// Respuesta: 404 {"success": false, "message": "La entidad 'Categoria' con id '123' no fue encontrada"}

// ValidationException - Validación fallida
throw new Common.Exceptions.ValidationException(errors);
// Respuesta: 400 {"success": false, "message": "Errores de validación", "errors": {...}}

// UnauthorizedException - No autorizado
throw new UnauthorizedException("Credenciales inválidas");
// Respuesta: 401 {"success": false, "message": "Credenciales inválidas"}

// DomainException - Error de negocio genérico
throw new DomainException("Usuario ya existe", "USER_ALREADY_EXISTS");
// Respuesta: 400 {"success": false, "message": "Usuario ya existe", "code": "USER_ALREADY_EXISTS"}
```

---

## 🚀 Extender Funcionalidad

### Ejemplo: Agregar Búsqueda por Rango de Fechas

#### Paso 1: Crear una Especificación

```csharp
// Archivo: Database/Repositories/Specifications/TransaccionesEntreFechasSpec.cs
public class TransaccionesEntreFechasSpec : Specification<Transaccion>
{
    public TransaccionesEntreFechasSpec(string usuarioId, DateTime desde, DateTime hasta)
    {
        Criteria = t => t.UsuarioId == usuarioId && 
                       t.Fecha >= desde && 
                       t.Fecha <= hasta;
        
        ApplyOrdering(t => t.Fecha); // Más recientes primero
        ApplyPaging(0, 50); // Paginado
    }
}
```

#### Paso 2: Agregar Método en el Servicio

```csharp
public interface ITransaccionService
{
    // Métodos existentes...
    Task<List<Transaccion>> GetPorRangoFechasAsync(string usuarioId, DateTime desde, DateTime hasta);
}

public class TransaccionService : ITransaccionService
{
    // Implementación existente...
    
    public async Task<List<Transaccion>> GetPorRangoFechasAsync(string usuarioId, DateTime desde, DateTime hasta)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentNullException(nameof(usuarioId));

        // ✅ Sin modificar código existente
        var spec = new TransaccionesEntreFechasSpec(usuarioId, desde, hasta);
        return await _repository.FindWithSpecAsync(spec);
    }
}
```

#### Paso 3: Exponer en el Controller

```csharp
[HttpGet("por-rango-fechas")]
public async Task<ActionResult<ApiResponse<List<Transaccion>>>> GetPorRangoFechas(
    [FromQuery] DateTime desde, 
    [FromQuery] DateTime hasta)
{
    try
    {
        var transacciones = await _service.GetPorRangoFechasAsync(usuarioId, desde, hasta);
        
        return Ok(new ApiResponse<List<Transaccion>>
        {
            Success = true,
            Data = transacciones,
            Message = $"Se encontraron {transacciones.Count} transacciones"
        });
    }
    catch (DomainException ex)
    {
        return BadRequest(new ApiResponse<List<Transaccion>>
        {
            Success = false,
            Message = ex.Message,
            Code = ex.Code
        });
    }
}
```

#### ✨ Ventaja: Sin Modificar Código Existente

- ✅ `MongoRepository<T>` - Sin cambios
- ✅ `TransaccionValidator` - Sin cambios
- ✅ `ITransaccionService` - Solo agregamos método
- ✅ `TransaccionService` - Solo agregamos implementación
- ✅ Middleware - Sin cambios
- ✅ Controllers - Solo agregamos nuevo endpoint

**Esto es el Principio Open/Closed en acción** 🎯

---

## 🧪 Testing con la Nueva Arquitectura

```csharp
[TestFixture]
public class CategoriaServiceTests
{
    private Mock<IRepository<Categoria>> _mockRepository;
    private Mock<IValidator<Categoria>> _mockValidator;
    private CategoriaService _service;

    [SetUp]
    public void SetUp()
    {
        _mockRepository = new Mock<IRepository<Categoria>>();
        _mockValidator = new Mock<IValidator<Categoria>>();
        _service = new CategoriaService(_mockRepository.Object, _mockValidator.Object);
    }

    [Test]
    public async Task CreateAsync_WithValidData_ShouldCreateCategoria()
    {
        // Arrange
        var categoria = new Categoria { Nombre = "Comida", Tipo = "Gasto", UsuarioId = "123" };
        var validationResult = new FluentValidation.Results.ValidationResult();
        
        _mockValidator
            .Setup(v => v.ValidateAsync(categoria, CancellationToken.None))
            .ReturnsAsync(validationResult);
        
        _mockRepository
            .Setup(r => r.AddAsync(categoria))
            .ReturnsAsync(categoria);

        // Act
        var result = await _service.CreateAsync(categoria);

        // Assert
        Assert.That(result.Nombre, Is.EqualTo("Comida"));
        _mockRepository.Verify(r => r.AddAsync(categoria), Times.Once);
    }

    [Test]
    public async Task CreateAsync_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var categoria = new Categoria { Nombre = "", Tipo = "Gasto", UsuarioId = "123" };
        var validationFailure = new ValidationFailure("Nombre", "El nombre es obligatorio");
        var validationResult = new FluentValidation.Results.ValidationResult(new[] { validationFailure });
        
        _mockValidator
            .Setup(v => v.ValidateAsync(categoria, CancellationToken.None))
            .ReturnsAsync(validationResult);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Common.Exceptions.ValidationException>(
            async () => await _service.CreateAsync(categoria)
        );
        Assert.That(ex.Code, Is.EqualTo("VALIDATION_ERROR"));
    }
}
```

---

## 📊 Resumen de Beneficios

| Aspecto | Antes | Después |
|--------|-------|---------|
| **Testing** | ❌ Difícil | ✅ Con mocks |
| **Reutilización** | ❌ CRUD duplicado | ✅ Genéricos |
| **Errores** | ❌ Inconsistentes | ✅ Estandarizados |
| **Validación** | ❌ En servicios | ✅ Centralizada |
| **Extensión** | ❌ Modificar existente | ✅ Nuevas clases |
| **Acoplamiento** | ❌ Alto (MongoDB) | ✅ Bajo (interfaces) |
| **Documentación** | ❌ Sin patrón | ✅ Código limpio |

---

¡Tu proyecto está listo para **producción** con arquitectura profesional! 🚀
