using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanzasPersonales.Models.DTOs;

/// <summary>
/// DTO para actualizar completamente una categoría (PUT)
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoriaUpdateDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    public string Nombre { get; set; } = null!;

    [Required(ErrorMessage = "El tipo es obligatorio (Ingreso o Gasto).")]
    [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'.")]
    public string Tipo { get; set; } = null!;
}
