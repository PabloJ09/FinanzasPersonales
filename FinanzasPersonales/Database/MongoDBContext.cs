
using System.Diagnostics.CodeAnalysis;
using MongoDB.Driver;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.Database
{
    [ExcludeFromCodeCoverage]
    public class MongoDBContext : IMongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        // Mantén pública la propiedad DatabaseName
        public string DatabaseName => _database.DatabaseNamespace.DatabaseName;

        // Implementación de la interfaz
        public IMongoCollection<Transaccion> Transacciones => _database.GetCollection<Transaccion>("Transacciones");
        public IMongoCollection<Categoria> Categorias => _database.GetCollection<Categoria>("Categorias");
        public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("Usuarios");
    }
}
