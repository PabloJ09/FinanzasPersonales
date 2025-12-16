namespace FinanzasPersonales.Models.DTOs.IA;

/// <summary>
/// DTO para confirmar y guardar la transacción procesada por IA
/// </summary>
public class ConfirmarTransaccionDto
{
    public CategoriaIADto Categoria { get; set; } = null!;
    public TransaccionIADto Transaccion { get; set; } = null!;
    public string UsuarioId { get; set; } = string.Empty;
}
