using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Database;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FinanzasPersonales.Validators;
using FluentValidation;

namespace FinanzasPersonales.Services;

/// <summary>
/// Servicio de dominio para Transacciones.
/// Principio: Single Responsibility - Solo lógica de transacciones
/// Principio: Dependency Inversion - Depende de IRepository, no de MongoDB directamente
/// </summary>
public interface ITransaccionService
{
    Task<List<Transaccion>> GetAllAsync(string userId);
    Task<Transaccion?> GetByIdAsync(string id, string userId);
    Task<Transaccion> CreateAsync(Transaccion transaccion);
    Task<Transaccion> UpdateAsync(string id, Transaccion transaccion, string userId);
    Task<List<Transaccion>> GetByUsuarioIdAsync(string usuarioId);
    Task<List<Transaccion>> GetByCategoriaAsync(string categoriaId);
    Task DeleteAsync(string id, string userId);
}

public class TransaccionService : ITransaccionService
{
    private readonly IRepository<Transaccion> _repository;
    private readonly IValidator<Transaccion> _validator;

    public TransaccionService(IRepository<Transaccion> repository, IValidator<Transaccion> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    // Backwards-compatible constructor used by older tests/code that passed IMongoDBContext
    internal TransaccionService(IMongoDBContext context)
        : this(new MongoRepository<Transaccion>(context.Transacciones), new Validators.TransaccionValidator())
    {
    }

    // Backwards-compatible method name
    public Task<List<Transaccion>> GetAsync() => GetAllAsync(string.Empty);

    public async Task<List<Transaccion>> GetAllAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _repository.FindAsync(t => t.UsuarioId == userId);
    }

    public async Task<Transaccion?> GetByIdAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Return null when not found to preserve test expectations
        var transacciones = await _repository.FindAsync(t => t.Id == id && t.UsuarioId == userId);
        return transacciones.FirstOrDefault();
    }

    public async Task<Transaccion> CreateAsync(Transaccion transaccion)
    {
        if (transaccion == null)
            throw new ArgumentNullException(nameof(transaccion));

        // Validar la entidad
        var validationResult = await _validator.ValidateAsync(transaccion);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        transaccion.Id = null;
        return await _repository.AddAsync(transaccion);
    }

    public async Task<Transaccion> UpdateAsync(string id, Transaccion transaccion, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (transaccion == null)
            throw new ArgumentNullException(nameof(transaccion));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Validate incoming model first (tests expect validation to occur before repository access)
        var validationResult = await _validator.ValidateAsync(transaccion);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        // Verify that the transaction belongs to the user
        var existing = await _repository.FindAsync(t => t.Id == id && t.UsuarioId == userId);
        if (!existing.Any())
            throw new KeyNotFoundException($"La entidad '{nameof(Transaccion)}' con id '{id}' no fue encontrada o no pertenece al usuario.");

        transaccion.Id = id;
        transaccion.UsuarioId = userId; // Ensure userId is not changed
        return await _repository.UpdateAsync(transaccion);
    }

    public async Task<List<Transaccion>> GetByUsuarioIdAsync(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentNullException(nameof(usuarioId));

        return await _repository.FindAsync(t => t.UsuarioId == usuarioId);
    }

    public async Task<List<Transaccion>> GetByCategoriaAsync(string categoriaId)
    {
        if (string.IsNullOrWhiteSpace(categoriaId))
            throw new ArgumentNullException(nameof(categoriaId));

        return await _repository.FindAsync(t => t.CategoriaId == categoriaId);
    }

    public async Task DeleteAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Verify that the transaction belongs to the user
        var existing = await _repository.FindAsync(t => t.Id == id && t.UsuarioId == userId);
        if (!existing.Any())
            throw new KeyNotFoundException($"La entidad '{nameof(Transaccion)}' con id '{id}' no fue encontrada o no pertenece al usuario.");

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"La entidad '{nameof(Transaccion)}' con id '{id}' no fue encontrada.");
    }
}
