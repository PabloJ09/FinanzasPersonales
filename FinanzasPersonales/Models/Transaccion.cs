using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace FinanzasPersonales.Models
{
    public class Transaccion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        public string Tipo { get; set; } = null!; // "Ingreso" o "Gasto"

        public decimal Monto { get; set; }

        public string Descripcion { get; set; } = null!;

        public string CategoriaId { get; set; } = null!;

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public string UsuarioId { get; set; } = null!;
    }
}
