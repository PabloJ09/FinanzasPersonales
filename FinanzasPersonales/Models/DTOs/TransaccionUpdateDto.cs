using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanzasPersonales.Models.DTOs;

/// <summary>
/// DTO para actualizar una transacción (PUT)
/// </summary>
[ExcludeFromCodeCoverage]
public class TransaccionUpdateDto
{
    [Required(ErrorMessage = "El tipo de transacción es obligatorio (Ingreso o Gasto).")]
    [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'.")]
    public string Tipo { get; set; } = null!;

    [Required(ErrorMessage = "El monto es obligatorio.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que 0.")]
    public decimal Monto { get; set; }

    [StringLength(200, ErrorMessage = "La descripción no puede exceder {1} caracteres.")]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es requerida.")]
    public string CategoriaId { get; set; } = null!;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime Fecha { get; set; }
}
