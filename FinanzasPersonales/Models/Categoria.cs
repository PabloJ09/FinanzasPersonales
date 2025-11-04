using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasPersonales.Models
{
    public class Categoria
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Nombre { get; set; } = null!;

        public string Tipo { get; set; } = null!; // "Ingreso" o "Gasto"

        public string UsuarioId { get; set; } = null!; // Categorías personalizadas por usuario
    }
}
