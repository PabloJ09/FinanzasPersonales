using FinanzasPersonales.Models;
using MongoDB.Driver;

namespace FinanzasPersonales.Database.Repositories;

/// <summary>
/// Implementación de Unit of Work para MongoDB.
/// Principio: Coordinator Pattern - Coordina múltiples repositorios
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IMongoDBContext _context;
    private IRepository<Categoria>? _categoriaRepository;
    private IRepository<Transaccion>? _transaccionRepository;
    private IRepository<Usuario>? _usuarioRepository;
    private IClientSessionHandle? _session;

    public UnitOfWork(IMongoDBContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<Categoria> CategoriaRepository
        => _categoriaRepository ??= new MongoRepository<Categoria>(_context.Categorias);

    public IRepository<Transaccion> TransaccionRepository
        => _transaccionRepository ??= new MongoRepository<Transaccion>(_context.Transacciones);

    public IRepository<Usuario> UsuarioRepository
        => _usuarioRepository ??= new MongoRepository<Usuario>(_context.Usuarios);

    public async Task<bool> SaveChangesAsync()
    {
        // MongoDB no requiere SaveChanges explícito, pero mantenemos el patrón
        return await Task.FromResult(true);
    }

    public async Task<bool> BeginTransactionAsync()
    {
        try
        {
            // Obtener el cliente MongoDB del contexto
            var client = _context.Categorias.Database.Client;
            _session = await client.StartSessionAsync();
            _session.StartTransaction();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> CommitTransactionAsync()
    {
        try
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.CommitTransactionAsync();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RollbackTransactionAsync()
    {
        try
        {
            if (_session != null && _session.IsInTransaction)
            {
                await _session.AbortTransactionAsync();
            }
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}
