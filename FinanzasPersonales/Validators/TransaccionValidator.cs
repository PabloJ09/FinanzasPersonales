using FluentValidation;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.Validators;

/// <summary>
/// Validador de Transacción usando FluentValidation.
/// Principio: Single Responsibility - Solo valida transacciones
/// </summary>
public class TransaccionValidator : AbstractValidator<Transaccion>
{
    public TransaccionValidator()
    {
        RuleFor(t => t.Tipo)
            .NotEmpty().WithMessage("El tipo de transacción es obligatorio.")
            .Must(t => t == "Ingreso" || t == "Gasto")
            .WithMessage("El tipo debe ser 'Ingreso' o 'Gasto'.");

        RuleFor(t => t.Monto)
            .NotEmpty().WithMessage("El monto es obligatorio.")
            .GreaterThan(0).WithMessage("El monto debe ser mayor que 0.");

        RuleFor(t => t.Descripcion)
            .MaximumLength(200).WithMessage("La descripción no puede exceder 200 caracteres.");

        RuleFor(t => t.CategoriaId)
            .NotEmpty().WithMessage("La categoría es requerida.")
            .Length(24).When(t => !string.IsNullOrEmpty(t.CategoriaId))
            .WithMessage("CategoriaId debe ser un ObjectId válido.");

        RuleFor(t => t.UsuarioId)
            .NotEmpty().WithMessage("UsuarioId es requerido.")
            .Length(24).When(t => !string.IsNullOrEmpty(t.UsuarioId))
            .WithMessage("UsuarioId debe ser un ObjectId válido.");

        RuleFor(t => t.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("La fecha no puede ser en el futuro.");
    }
}
