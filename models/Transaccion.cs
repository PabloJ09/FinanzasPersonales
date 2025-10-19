using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FinanzasPersonales.Models
{
    public class Transaccion
    {
        //Esta es una prueba 2
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("tipo")]
        public string Tipo { get; set; } = string.Empty; // "Ingreso" o "Gasto"

        [BsonElement("monto")]
        public decimal Monto { get; set; }

        [BsonElement("categoria")]
        public string Categoria { get; set; } = string.Empty;

        [BsonElement("descripcion")]
        public string Descripcion { get; set; } = string.Empty;

        [BsonElement("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}
