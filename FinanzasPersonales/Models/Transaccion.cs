using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

using System.Diagnostics.CodeAnalysis;
namespace FinanzasPersonales.Models
{
    [ExcludeFromCodeCoverage]
    public class Transaccion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [Required(ErrorMessage = "El tipo de transacción es obligatorio (Ingreso o Gasto).")]
        [RegularExpression("^(Ingreso|Gasto)$", ErrorMessage = "El tipo debe ser 'Ingreso' o 'Gasto'.")]
        public string Tipo { get; set; } = null!; // "Ingreso" o "Gasto"

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor que 0.")]
        public decimal Monto { get; set; }

        [StringLength(200, ErrorMessage = "La descripción no puede exceder {1} caracteres.")]
        public string Descripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "La categoría es requerida.")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CategoriaId { get; set; } = null!;        [Required(ErrorMessage = "La fecha es obligatoria.")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "UsuarioId es requerido.")]
        public string UsuarioId { get; set; } = null!; // Viene del JWT
    }
}
