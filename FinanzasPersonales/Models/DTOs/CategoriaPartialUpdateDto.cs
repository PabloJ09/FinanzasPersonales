using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace FinanzasPersonales.Models.DTOs;

/// <summary>
/// DTO para actualizar parcialmente una categoría (PATCH)
/// </summary>
[ExcludeFromCodeCoverage]
public class CategoriaPartialUpdateDto
{
    [StringLength(50, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
    public string? Nombre { get; set; }

    [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'.")]
    public string? Tipo { get; set; }
}
