using FluentValidation;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.Validators;

/// <summary>
/// Validador de Usuario usando FluentValidation.
/// Principio: Single Responsibility - Solo valida usuarios
/// </summary>
public class UsuarioValidator : AbstractValidator<Usuario>
{
    public UsuarioValidator()
    {
        RuleFor(u => u.Username)
            .NotEmpty().WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3).WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50).WithMessage("El nombre de usuario no puede exceder 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_\-]+$")
            .WithMessage("El nombre de usuario solo puede contener letras, números, guiones y guiones bajos.");

        RuleFor(u => u.PasswordHash)
            .NotEmpty().WithMessage("La contraseña es obligatoria.");

        RuleFor(u => u.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(r => r == "admin" || r == "usuario")
            .WithMessage("El rol debe ser 'admin' o 'usuario'.");
    }
}
