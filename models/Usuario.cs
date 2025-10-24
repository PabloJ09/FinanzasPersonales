using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using BCrypt.Net;

namespace FinanzasPersonales.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("nombre")]
        public string Nombre { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("passwordHash")]
        public string PasswordHash { get; private set; } = null!;

        [BsonElement("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        // Hashea la contraseña usando bcrypt
        public void SetPassword(string password)
        {
            // WorkFactor = costo computacional (recomendado 10–12 para servidores normales)
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        // Verifica si una contraseña coincide con el hash
        public bool VerifyPassword(string password)
        {
            return BCrypt.Net.BCrypt.Verify(password, PasswordHash);
        }
    }
}
