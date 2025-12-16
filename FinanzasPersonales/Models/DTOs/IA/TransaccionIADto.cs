namespace FinanzasPersonales.Models.DTOs.IA;

/// <summary>
/// DTO que representa la transacción estructurada por la IA
/// </summary>
public class TransaccionIADto
{
    public string? Id { get; set; }
    public string? Tipo { get; set; } // "Ingreso" o "Gasto"
    public decimal? Monto { get; set; }
    public string? Descripcion { get; set; }
    public string? CategoriaId { get; set; }
    public DateTime? Fecha { get; set; }
    public string? UsuarioId { get; set; }
}
