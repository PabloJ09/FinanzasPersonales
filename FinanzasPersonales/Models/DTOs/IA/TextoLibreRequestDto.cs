namespace FinanzasPersonales.Models.DTOs.IA;

/// <summary>
/// DTO para recibir el texto libre del usuario que será procesado por IA
/// </summary>
public class TextoLibreRequestDto
{
    /// <summary>
    /// Texto libre escrito por el usuario para crear una transacción
    /// </summary>
    public string Texto { get; set; } = string.Empty;

    /// <summary>
    /// ID del usuario autenticado (se obtiene del token)
    /// </summary>
    public string UsuarioId { get; set; } = string.Empty;
}
