using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;

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
            // Eliminamos el id si viene definido para que MongoDB lo genere automáticamente
            categoria.Id = null;
            await _categorias.InsertOneAsync(categoria);
            return categoria;
        }

        // Actualizar completamente (PUT)
        public async Task UpdateAsync(string id, Categoria categoria)
        {
            categoria.Id = id; // Aseguramos que el id del objeto sea el de la ruta
            await _categorias.ReplaceOneAsync(c => c.Id == id, categoria);
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

            await _categorias.ReplaceOneAsync(c => c.Id == id, existing);
            return existing;
        }

        // Eliminar categoría
        public async Task DeleteAsync(string id) =>
            await _categorias.DeleteOneAsync(c => c.Id == id);
    }
}
