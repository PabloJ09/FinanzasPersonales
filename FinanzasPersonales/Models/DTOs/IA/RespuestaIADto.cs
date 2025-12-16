namespace FinanzasPersonales.Models.DTOs.IA;

/// <summary>
/// DTO que contiene la respuesta estructurada de la IA para revisión del usuario
/// </summary>
public class RespuestaIADto
{
    public CategoriaIADto? Categoria { get; set; }
    public TransaccionIADto? Transaccion { get; set; }
    public string? TextoOriginal { get; set; }
    public List<string> Advertencias { get; set; } = new();
}
