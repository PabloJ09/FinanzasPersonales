namespace FinanzasPersonales.Common.Results;

/// <summary>
/// Respuesta de API estandarizada.
/// Principio: Interface Segregation - Define contrato mínimo para respuestas
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> FromResult(Result<T> result)
        => new()
        {
            Success = result.IsSuccess,
            Data = result.Data,
            Message = result.Message,
            Code = result.Code,
            Errors = result.Errors,
            Timestamp = DateTime.UtcNow
        };
}

/// <summary>
/// Respuesta de API sin dato genérico
/// </summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse FromResult(Result result)
        => new()
        {
            Success = result.IsSuccess,
            Message = result.Message,
            Code = result.Code,
            Errors = result.Errors,
            Timestamp = DateTime.UtcNow
        };
}
