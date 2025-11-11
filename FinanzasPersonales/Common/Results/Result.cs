namespace FinanzasPersonales.Common.Results;

/// <summary>
/// Patrón Result genérico para manejar éxito y fracaso sin lanzar excepciones.
/// Principio: Dependency Inversion - Todos los resultados derivan de esta abstracción
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public int? StatusCode { get; set; }

    // Constructor privado para forzar usar los métodos estáticos
    private Result(bool isSuccess, T? data, string? message, string? code, 
                   Dictionary<string, string[]>? errors, int? statusCode)
    {
        IsSuccess = isSuccess;
        Data = data;
        Message = message;
        Code = code;
        Errors = errors;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Crea un resultado exitoso
    /// </summary>
    public static Result<T> Success(T data, string? message = null, int statusCode = 200)
        => new(true, data, message, "SUCCESS", null, statusCode);

    /// <summary>
    /// Crea un resultado fallido con un mensaje
    /// </summary>
    public static Result<T> Failure(string message, string code = "ERROR", int statusCode = 400, 
                                     Dictionary<string, string[]>? errors = null)
        => new(false, default, message, code, errors, statusCode);

    /// <summary>
    /// Crea un resultado cuando la entidad no es encontrada
    /// </summary>
    public static Result<T> NotFound(string message, string code = "ENTITY_NOT_FOUND")
        => new(false, default, message, code, null, 404);

    /// <summary>
    /// Crea un resultado de validación fallida
    /// </summary>
    public static Result<T> ValidationFailure(Dictionary<string, string[]> errors)
        => new(false, default, "Errores de validación", "VALIDATION_ERROR", errors, 400);

    /// <summary>
    /// Crea un resultado de no autorizado
    /// </summary>
    public static Result<T> Unauthorized(string message = "No autorizado")
        => new(false, default, message, "UNAUTHORIZED", null, 401);

    /// <summary>
    /// Crea un resultado de acceso prohibido
    /// </summary>
    public static Result<T> Forbidden(string message = "Acceso prohibido")
        => new(false, default, message, "FORBIDDEN", null, 403);

    /// <summary>
    /// Transforma el resultado actual a otro tipo
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T?, TNew> mapper)
    {
        if (!IsSuccess)
            return Result<TNew>.Failure(Message!, Code!, StatusCode ?? 400, Errors);

        try
        {
            var mappedData = mapper(Data);
            return Result<TNew>.Success(mappedData, Message, StatusCode ?? 200);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex.Message, "MAPPING_ERROR", 500);
        }
    }

    /// <summary>
    /// Encadena operaciones asincrónicas
    /// </summary>
    public async Task<Result<TNew>> BindAsync<TNew>(Func<T?, Task<Result<TNew>>> binder)
    {
        if (!IsSuccess)
            return Result<TNew>.Failure(Message!, Code!, StatusCode ?? 400, Errors);

        try
        {
            return await binder(Data);
        }
        catch (Exception ex)
        {
            return Result<TNew>.Failure(ex.Message, "BIND_ERROR", 500);
        }
    }
}

/// <summary>
/// Resultado sin dato genérico
/// </summary>
public class Result
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public int? StatusCode { get; set; }

    private Result(bool isSuccess, string? message, string? code, 
                   Dictionary<string, string[]>? errors, int? statusCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        Code = code;
        Errors = errors;
        StatusCode = statusCode;
    }

    public static Result Success(string? message = null, int statusCode = 200)
        => new(true, message, "SUCCESS", null, statusCode);

    public static Result Failure(string message, string code = "ERROR", int statusCode = 400, 
                                 Dictionary<string, string[]>? errors = null)
        => new(false, message, code, errors, statusCode);

    public static Result NotFound(string message)
        => new(false, message, "ENTITY_NOT_FOUND", null, 404);

    public static Result ValidationFailure(Dictionary<string, string[]> errors)
        => new(false, "Errores de validación", "VALIDATION_ERROR", errors, 400);

    public static Result Unauthorized(string message = "No autorizado")
        => new(false, message, "UNAUTHORIZED", null, 401);

    public static Result Forbidden(string message = "Acceso prohibido")
        => new(false, message, "FORBIDDEN", null, 403);
}
