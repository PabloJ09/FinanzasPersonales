using FinanzasPersonales.Models;

namespace FinanzasPersonales.Database.Repositories;

/// <summary>
/// Interfaz de Unit of Work.
/// Principio: Dependency Inversion - Abstrae la coordinación de repositorios
/// Principio: Single Responsibility - Coordina múltiples repositorios en una transacción
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IRepository<Categoria> CategoriaRepository { get; }
    IRepository<Transaccion> TransaccionRepository { get; }
    IRepository<Usuario> UsuarioRepository { get; }

    Task<bool> SaveChangesAsync();
    Task<bool> BeginTransactionAsync();
    Task<bool> CommitTransactionAsync();
    Task<bool> RollbackTransactionAsync();
}
