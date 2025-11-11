using MongoDB.Bson;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace FinanzasPersonales.Database.Repositories;

/// <summary>
/// Implementación genérica del repositorio para MongoDB.
/// Principio: DRY (Don't Repeat Yourself) - Código de CRUD centralizado
/// Principio: Open/Closed - Abierto a extensión (herencia), cerrado a modificación
/// </summary>
public class MongoRepository<T> : IRepository<T> where T : class
{
    protected readonly IMongoCollection<T> _collection;

    public MongoRepository(IMongoCollection<T> collection)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
    }

    public async Task<T?> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (!ObjectId.TryParse(id, out _))
            return null;

        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return await _collection.Find(predicate).FirstOrDefaultAsync();
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        return await _collection.Find(predicate).ToListAsync();
    }

    public async Task<List<T>> GetAllAsync()
    {
        return await _collection.Find(_ => true).ToListAsync();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        var count = await _collection.CountDocumentsAsync(predicate ?? (_ => true));
        return (int)count;
    }

    public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var count = await _collection.CountDocumentsAsync(predicate);
        return count > 0;
    }

    public async Task<T> AddAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        // Obtener el ID del documento
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
            throw new InvalidOperationException($"La entidad {typeof(T).Name} no tiene propiedad 'Id'");

        var id = idProperty.GetValue(entity)?.ToString();
        if (string.IsNullOrWhiteSpace(id))
            throw new InvalidOperationException("El ID de la entidad no puede estar vacío");

        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        var options = new ReplaceOptions { IsUpsert = false };
        var result = await _collection.ReplaceOneAsync(filter, entity, options);

        if (result.MatchedCount == 0)
            throw new InvalidOperationException($"No se encontró entidad con ID '{id}'");

        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (!ObjectId.TryParse(id, out _))
            return false;

        var filter = Builders<T>.Filter.Eq("_id", ObjectId.Parse(id));
        var result = await _collection.DeleteOneAsync(filter);
        return result.DeletedCount > 0;
    }

    public async Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var result = await _collection.DeleteManyAsync(predicate);
        return (int)result.DeletedCount;
    }

    public async Task<List<T>> FindWithPaginationAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        if (pageNumber < 1)
            throw new ArgumentException("El número de página debe ser mayor a 0", nameof(pageNumber));

        if (pageSize < 1)
            throw new ArgumentException("El tamaño de página debe ser mayor a 0", nameof(pageSize));

        var skip = (pageNumber - 1) * pageSize;
        return await _collection
            .Find(predicate)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
    }
}
