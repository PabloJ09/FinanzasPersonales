using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

using System.Diagnostics.CodeAnalysis;
namespace FinanzasPersonales.Models
{
    [ExcludeFromCodeCoverage]
    public class Categoria
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder {1} caracteres.")]
        public string Nombre { get; set; } = null!;

        [Required(ErrorMessage = "El tipo es obligatorio (Ingreso o Gasto).")]
        [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'.")]
        public string Tipo { get; set; } = null!; // "Ingreso" o "Gasto"

        [Required(ErrorMessage = "UsuarioId es requerido.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string UsuarioId { get; set; } = null!; // Categorías personalizadas por usuario
    }
}
