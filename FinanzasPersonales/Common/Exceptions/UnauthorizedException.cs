namespace FinanzasPersonales.Common.Exceptions;

/// <summary>
/// Excepción lanzada cuando hay errores de autenticación o autorización.
/// Principio: Single Responsibility - Solo maneja errores de seguridad
/// </summary>
public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "No autorizado")
        : base(message, "UNAUTHORIZED")
    {
    }
}
