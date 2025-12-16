namespace FinanzasPersonales.Models.DTOs.IA;

/// <summary>
/// DTO que representa la categoría estructurada por la IA
/// </summary>
public class CategoriaIADto
{
    public string? Id { get; set; }
    public string? Nombre { get; set; }
    public string? Tipo { get; set; } // "Ingreso" o "Gasto"
    public string? UsuarioId { get; set; }
}
