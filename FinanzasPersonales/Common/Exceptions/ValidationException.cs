namespace FinanzasPersonales.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay errores de validación.
/// Principio: Single Responsibility - Solo maneja errores de validación
/// </summary>
public class ValidationException : DomainException
{
    public ValidationException(Dictionary<string, string[]> errors)
        : base("Errores de validación encontrados.", "VALIDATION_ERROR", errors)
    {
    }

    public ValidationException(string property, params string[] messages)
        : base("Errores de validación encontrados.", "VALIDATION_ERROR", 
            new Dictionary<string, string[]> { { property, messages } })
    {
    }
}
