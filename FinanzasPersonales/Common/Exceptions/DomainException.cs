namespace FinanzasPersonales.Common.Exceptions;

/// <summary>
/// Excepción base para errores de dominio.
/// Principio: Single Responsibility - Solo maneja errores de lógica de negocio
/// </summary>
public class DomainException : Exception
{
    public string Code { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }

    public DomainException(string message, string code = "DOMAIN_ERROR", Dictionary<string, string[]>? errors = null)
        : base(message)
    {
        Code = code;
        Errors = errors;
    }

    public DomainException(string message, Exception innerException, string code = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        Code = code;
    }
}
