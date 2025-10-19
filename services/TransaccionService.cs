using FinanzasPersonales.Models;
using MongoDB.Driver;
using DotNetEnv;

namespace FinanzasPersonales.Services
{
    public class TransaccionService
    {
        private readonly IMongoCollection<Transaccion> _transacciones;

        public TransaccionService()
        {
            // Cargar variables de entorno
            Env.Load();

            var mongoUri = Environment.GetEnvironmentVariable("MONGO_URI");
            var mongoDbName = Environment.GetEnvironmentVariable("MONGO_DB");

            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(mongoDbName);

            _transacciones = database.GetCollection<Transaccion>("Transacciones");
        }

        public async Task<List<Transaccion>> GetAsync() =>
            await _transacciones.Find(_ => true).ToListAsync();

        public async Task<Transaccion> CreateAsync(Transaccion transaccion)
        {
            await _transacciones.InsertOneAsync(transaccion);
            return transaccion;
        }

        public async Task DeleteAsync(string id) =>
            await _transacciones.DeleteOneAsync(t => t.Id == id);
    }
}
