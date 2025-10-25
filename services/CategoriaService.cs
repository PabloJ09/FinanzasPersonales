using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;

namespace FinanzasPersonales.Services
{
    public class CategoriaService
    {
        private readonly IMongoCollection<Categoria> _categorias;

        public CategoriaService(MongoDBContext context)
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
            await _categorias.InsertOneAsync(categoria);
            return categoria;
        }

        // Actualizar categoría existente
        public async Task UpdateAsync(string id, Categoria categoria) =>
            await _categorias.ReplaceOneAsync(c => c.Id == id, categoria);

        // Eliminar categoría
        public async Task DeleteAsync(string id) =>
            await _categorias.DeleteOneAsync(c => c.Id == id);
    }
}
