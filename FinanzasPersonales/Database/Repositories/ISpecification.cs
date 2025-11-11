using System.Linq.Expressions;

namespace FinanzasPersonales.Database.Repositories;

/// <summary>
/// Interfaz de Specification para implementar búsquedas complejas.
/// Principio: Open/Closed - Permite agregar búsquedas sin modificar repositorio
/// Principio: Single Responsibility - Cada specification define una búsqueda específica
/// </summary>
public interface ISpecification<T> where T : class
{
    Expression<Func<T, bool>> Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    int? Take { get; }
    int? Skip { get; }
    bool IsPagingEnabled { get; }
}

/// <summary>
/// Clase base abstracta para Specifications.
/// Proporciona funcionalidad común para definir búsquedas.
/// </summary>
public abstract class Specification<T> : ISpecification<T> where T : class
{
    public Expression<Func<T, bool>> Criteria { get; protected set; } = null!;
    public List<Expression<Func<T, object>>> Includes { get; } = new();
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public int? Take { get; protected set; }
    public int? Skip { get; protected set; }
    public bool IsPagingEnabled { get; protected set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrdering(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderingDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
}
