using MongoDB.Driver;
using System.Linq.Expressions;

namespace FinanzasPersonales.Database.Repositories;

/// <summary>
/// Interfaz genérica de repositorio.
/// Principio: Dependency Inversion - Abstraer el acceso a datos
/// Principio: Interface Segregation - Métodos específicos, no todo mezclado
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<List<T>> GetAllAsync();
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<int> DeleteManyAsync(Expression<Func<T, bool>> predicate);
    
    Task<List<T>> FindWithPaginationAsync(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);
}
