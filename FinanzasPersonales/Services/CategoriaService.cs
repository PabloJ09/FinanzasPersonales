using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Database;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FinanzasPersonales.Validators;
using FluentValidation;

namespace FinanzasPersonales.Services;

/// <summary>
/// Servicio de dominio para Categorías.
/// Principio: Single Responsibility - Solo lógica de categorías
/// Principio: Dependency Inversion - Depende de IRepository, no de MongoDB directamente
/// </summary>
public interface ICategoriaService
{
    Task<List<Categoria>> GetAllAsync(string userId);
    Task<Categoria?> GetByIdAsync(string id, string userId);
    Task<Categoria> CreateAsync(Categoria categoria);
    Task<Categoria> UpdateAsync(string id, Categoria categoria, string userId);
    Task<Categoria?> UpdatePartialAsync(string id, Categoria partial, string userId);
    Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId);
    Task DeleteAsync(string id, string userId);
}

public class CategoriaService : ICategoriaService
{
    private readonly IRepository<Categoria> _repository;
    private readonly IValidator<Categoria> _validator;

    public CategoriaService(IRepository<Categoria> repository, IValidator<Categoria> validator)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    // Backwards-compatible constructor used by older tests/code that passed IMongoDBContext
    internal CategoriaService(IMongoDBContext context)
        : this(new MongoRepository<Categoria>(context.Categorias), new Validators.CategoriaValidator())
    {
    }

    public async Task<List<Categoria>> GetAllAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        return await _repository.FindAsync(c => c.UsuarioId == userId);
    }

    // Backwards-compatible method name
    public Task<List<Categoria>> GetAsync() => GetAllAsync(string.Empty);

    public async Task<Categoria?> GetByIdAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Return null when not found to preserve previous behavior expected by tests
        var categorias = await _repository.FindAsync(c => c.Id == id && c.UsuarioId == userId);
        return categorias.FirstOrDefault();
    }

    public async Task<Categoria> CreateAsync(Categoria categoria)
    {
        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));

        // Validar la entidad
        var validationResult = await _validator.ValidateAsync(categoria);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        categoria.Id = null;
        return await _repository.AddAsync(categoria);
    }

    public async Task<Categoria> UpdateAsync(string id, Categoria categoria, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Validate incoming model first (tests expect validation to occur before repository access)
        var validationResult = await _validator.ValidateAsync(categoria);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        // Verify that the category belongs to the user
        var existing = await _repository.FindAsync(c => c.Id == id && c.UsuarioId == userId);
        if (!existing.Any())
            throw new KeyNotFoundException($"La entidad '{nameof(Categoria)}' con id '{id}' no fue encontrada o no pertenece al usuario.");

        categoria.Id = id;
        categoria.UsuarioId = userId; // Ensure userId is not changed
        return await _repository.UpdateAsync(categoria);
    }

    public async Task<Categoria?> UpdatePartialAsync(string id, Categoria partial, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (partial == null)
            throw new ArgumentNullException(nameof(partial));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Verify that the category belongs to the user
        var existingList = await _repository.FindAsync(c => c.Id == id && c.UsuarioId == userId);
        var existing = existingList.FirstOrDefault();
        if (existing == null)
            // Tests expect null when an entity is not found for partial update
            return null!;

        // Aplicar cambios parciales
        if (!string.IsNullOrEmpty(partial.Nombre))
            existing.Nombre = partial.Nombre;

        if (!string.IsNullOrEmpty(partial.Tipo))
            existing.Tipo = partial.Tipo;

        // UsuarioId should NOT be updated for security
        // Keep the existing userId

        // Validar el objeto actualizado
        var validationResult = await _validator.ValidateAsync(existing);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        return await _repository.UpdateAsync(existing);
    }

    public async Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentNullException(nameof(usuarioId));

        return await _repository.FindAsync(c => c.UsuarioId == usuarioId);
    }

    public async Task DeleteAsync(string id, string userId)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        // Verify that the category belongs to the user
        var existing = await _repository.FindAsync(c => c.Id == id && c.UsuarioId == userId);
        if (!existing.Any())
            throw new KeyNotFoundException($"La entidad '{nameof(Categoria)}' con id '{id}' no fue encontrada o no pertenece al usuario.");

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new KeyNotFoundException($"La entidad '{nameof(Categoria)}' con id '{id}' no fue encontrada.");
    }
}
