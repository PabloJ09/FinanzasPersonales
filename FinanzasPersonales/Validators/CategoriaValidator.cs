using FluentValidation;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.Validators;

/// <summary>
/// Validador de Categoría usando FluentValidation.
/// Principio: Single Responsibility - Solo valida categorías
/// </summary>
public class CategoriaValidator : AbstractValidator<Categoria>
{
    public CategoriaValidator()
    {
        RuleFor(c => c.Nombre)
            .NotEmpty().WithMessage("El nombre de la categoría es obligatorio.")
            .MaximumLength(50).WithMessage("El nombre no puede exceder 50 caracteres.");

        RuleFor(c => c.Tipo)
            .NotEmpty().WithMessage("El tipo es obligatorio.")
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("El tipo debe ser 'Ingreso' o 'Gasto'.");

        RuleFor(c => c.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId es requerido.");
    }
}
