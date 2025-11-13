namespace FinanzasPersonales.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics.CodeAnalysis;
    [ExcludeFromCodeCoverage]

    public class Usuario
    {
        [MongoDB.Bson.Serialization.Attributes.BsonId]
        [MongoDB.Bson.Serialization.Attributes.BsonRepresentation(MongoDB.Bson.BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "El nombre de usuario debe tener entre {2} y {1} caracteres.")]
        public string Username { get; set; } = string.Empty;

        // Almacenamos salt + hash en formato base64Salt:base64Hash para no cambiar la interfaz solicitada
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public bool IsActive { get; set; } = false;

        [Required(ErrorMessage = "El rol es obligatorio.")]
    [RegularExpression("(?i)^(admin|usuario)$", ErrorMessage = "El rol debe ser 'admin' o 'usuario'.")]
        public string Role { get; set; } = "usuario";
    }
}