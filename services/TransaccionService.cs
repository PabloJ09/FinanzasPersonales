using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;

namespace FinanzasPersonales.Services
{
    public class TransaccionService
    {
        private readonly IMongoCollection<Transaccion> _transacciones;

        public TransaccionService(MongoDBContext context)
        {
            _transacciones = context.Transacciones;
        }

        // Obtener todas las transacciones
        public async Task<List<Transaccion>> GetAsync() =>
            await _transacciones.Find(_ => true).ToListAsync();

        // Obtener transacción por Id
        public async Task<Transaccion?> GetByIdAsync(string id) =>
            await _transacciones.Find(t => t.Id == id).FirstOrDefaultAsync();

        // Crear nueva transacción
        public async Task<Transaccion> CreateAsync(Transaccion transaccion)
        {
            await _transacciones.InsertOneAsync(transaccion);
            return transaccion;
        }

        // Actualizar transacción existente
        public async Task UpdateAsync(string id, Transaccion transaccion) =>
            await _transacciones.ReplaceOneAsync(t => t.Id == id, transaccion);

        // Eliminar transacción
        public async Task DeleteAsync(string id) =>
            await _transacciones.DeleteOneAsync(t => t.Id == id);
    }
}
