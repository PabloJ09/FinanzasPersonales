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

        FilterDefinition<T> filter;
        if (ObjectId.TryParse(id, out var oid))
        {
            filter = Builders<T>.Filter.Eq("_id", oid);
        }
        else
        {
            // Fallback to string Id field so tests which use simple ids (like "1") continue to work
            filter = Builders<T>.Filter.Eq("Id", id);
        }
        var cursor = await _collection.FindAsync(filter, null, System.Threading.CancellationToken.None);
        if (cursor == null)
            return null;

        // Iterate through cursor pages defensively. Some test mocks may return a cursor
        // whose Current is null or empty; guard against NullReferenceException.
        while (await cursor.MoveNextAsync())
        {
            var current = cursor.Current;
            if (current != null)
            {
                var first = current.FirstOrDefault();
                if (first != null)
                    return first;
            }
        }

        return null;
    }

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var cursor = await _collection.FindAsync(predicate, null, System.Threading.CancellationToken.None);
        if (await cursor.MoveNextAsync())
            return cursor.Current.FirstOrDefault();
        return null;
    }

    public async Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        if (predicate == null)
            throw new ArgumentNullException(nameof(predicate));

        var cursor = await _collection.FindAsync(predicate, null, System.Threading.CancellationToken.None);
        var results = new List<T>();
        while (await cursor.MoveNextAsync())
        {
            results.AddRange(cursor.Current);
        }
        return results;
    }

    public async Task<List<T>> GetAllAsync()
    {
        var cursor = await _collection.FindAsync(_ => true, null, System.Threading.CancellationToken.None);
        var results = new List<T>();
        while (await cursor.MoveNextAsync())
        {
            results.AddRange(cursor.Current);
        }
        return results;
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

        // Use FindAsync to cooperate with tests that mock FindAsync on IMongoCollection
        var cursor = await _collection.FindAsync(predicate, null, System.Threading.CancellationToken.None);
        if (await cursor.MoveNextAsync())
            return cursor.Current.Any();
        return false;
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

        // Support both ObjectId-based _id and string Id field used in tests
        FilterDefinition<T> filter;
        if (ObjectId.TryParse(id, out var oid))
        {
            filter = Builders<T>.Filter.Eq("_id", oid);
        }
        else
        {
            filter = Builders<T>.Filter.Eq("Id", id);
        }

        var options = new ReplaceOptions { IsUpsert = false };
        var result = await _collection.ReplaceOneAsync(filter, entity, options);

        if (result == null || result.MatchedCount == 0)
            throw new KeyNotFoundException($"No se encontró entidad con ID '{id}'");

        return entity;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        FilterDefinition<T> filter;
        if (ObjectId.TryParse(id, out var oid))
        {
            filter = Builders<T>.Filter.Eq("_id", oid);
        }
        else
        {
            // Fallback to string Id field
            filter = Builders<T>.Filter.Eq("Id", id);
        }

        var result = await _collection.DeleteOneAsync(filter);
        if (result == null)
            return false;

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
