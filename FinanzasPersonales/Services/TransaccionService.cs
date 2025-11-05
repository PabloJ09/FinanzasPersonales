using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Services
{
    public class TransaccionService
    {
        private readonly IMongoCollection<Transaccion> _transacciones;

        public TransaccionService(IMongoDBContext context)
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
            Validator.ValidateObject(transaccion, new ValidationContext(transaccion), validateAllProperties: true);
            transaccion.Id = null;
            await _transacciones.InsertOneAsync(transaccion);
            return transaccion;
        }

        // Actualizar transacción existente
        public async Task UpdateAsync(string id, Transaccion transaccion)
        {
            Validator.ValidateObject(transaccion, new ValidationContext(transaccion), validateAllProperties: true);
            transaccion.Id = id;
            var result = await _transacciones.ReplaceOneAsync(t => t.Id == id, transaccion);
            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Transacción con id {id} no encontrada.");
        }

        // Eliminar transacción
        public async Task DeleteAsync(string id)
        {
            var result = await _transacciones.DeleteOneAsync(t => t.Id == id);
            if (result.DeletedCount == 0)
                throw new KeyNotFoundException($"Transacción con id {id} no encontrada.");
        }

        // Obtener transacciones por usuario
        public async Task<List<Transaccion>> GetByUsuarioIdAsync(string usuarioId) =>
            await _transacciones.Find(t => t.UsuarioId == usuarioId).ToListAsync();

        // Obtener transacciones por categoría
        public async Task<List<Transaccion>> GetByCategoriaAsync(string categoriaId) =>
            await _transacciones.Find(t => t.CategoriaId == categoriaId).ToListAsync();
    }
}
