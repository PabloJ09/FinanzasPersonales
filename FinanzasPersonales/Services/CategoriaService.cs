using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

namespace FinanzasPersonales.Services
{
    public class CategoriaService
    {
        private readonly IMongoCollection<Categoria> _categorias;

        public CategoriaService(IMongoDBContext context)
        {
            _categorias = context.Categorias;
        }

        // Obtener todas las categorías
        public async Task<List<Categoria>> GetAsync() =>
            await _categorias.Find(_ => true).ToListAsync();

        // Obtener categoría por Id
        public async Task<Categoria?> GetByIdAsync(string id) =>
            await _categorias.Find(c => c.Id == id).FirstOrDefaultAsync();

        // Crear nueva categoría
        public async Task<Categoria> CreateAsync(Categoria categoria)
        {
            Validator.ValidateObject(categoria, new ValidationContext(categoria), validateAllProperties: true);
            categoria.Id = null;
            await _categorias.InsertOneAsync(categoria);
            return categoria;
        }

        // Actualizar completamente (PUT)
        public async Task UpdateAsync(string id, Categoria categoria)
        {
            Validator.ValidateObject(categoria, new ValidationContext(categoria), validateAllProperties: true);
            categoria.Id = id;
            var result = await _categorias.ReplaceOneAsync(c => c.Id == id, categoria);
            if (result.MatchedCount == 0)
                throw new KeyNotFoundException($"Categoría con id {id} no encontrada.");
        }

        // Actualizar parcialmente (PATCH)
        public async Task<Categoria?> UpdatePartialAsync(string id, Categoria partial)
        {
            var existing = await _categorias.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (existing == null) return null;

            // Solo actualizamos los campos que tengan valor
            if (!string.IsNullOrEmpty(partial.Nombre))
                existing.Nombre = partial.Nombre;

            if (!string.IsNullOrEmpty(partial.Tipo))
                existing.Tipo = partial.Tipo;

            if (!string.IsNullOrEmpty(partial.UsuarioId))
                existing.UsuarioId = partial.UsuarioId;

            // Validar el objeto actualizado antes de guardar
            Validator.ValidateObject(existing, new ValidationContext(existing), validateAllProperties: true);
            
            await _categorias.ReplaceOneAsync(c => c.Id == id, existing);
            return existing;
        }

        // Obtener categorías por usuario
        public async Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId) =>
            await _categorias.Find(c => c.UsuarioId == usuarioId).ToListAsync();

        // Eliminar categoría
        public async Task DeleteAsync(string id)
        {
            var result = await _categorias.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
                throw new KeyNotFoundException($"Categoría con id {id} no encontrada.");
        }
    }
}
