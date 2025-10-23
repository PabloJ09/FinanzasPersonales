using MongoDB.Driver;
using FinanzasPersonales.Models;

namespace FinanzasPersonales.Database
{
    public class MongoDBContext
    {
        private readonly IMongoDatabase _database;

        public MongoDBContext(MongoDBSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }

        public IMongoCollection<Transaccion> Transacciones => _database.GetCollection<Transaccion>("Transacciones");
        public IMongoCollection<Categoria> Categorias => _database.GetCollection<Categoria>("Categorias");
        public IMongoCollection<Usuario> Usuarios => _database.GetCollection<Usuario>("Usuarios");
    }
}
