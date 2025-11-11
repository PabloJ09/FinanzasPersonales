using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FluentValidation;

namespace FinanzasPersonales.Services;

/// <summary>
/// Servicio de dominio para Categorías.
/// Principio: Single Responsibility - Solo lógica de categorías
/// Principio: Dependency Inversion - Depende de IRepository, no de MongoDB directamente
/// </summary>
public interface ICategoriaService
{
    Task<List<Categoria>> GetAllAsync();
    Task<Categoria> GetByIdAsync(string id);
    Task<Categoria> CreateAsync(Categoria categoria);
    Task<Categoria> UpdateAsync(string id, Categoria categoria);
    Task<Categoria> UpdatePartialAsync(string id, Categoria partial);
    Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId);
    Task DeleteAsync(string id);
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

    public async Task<List<Categoria>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Categoria> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var categoria = await _repository.GetByIdAsync(id);
        if (categoria == null)
            throw new EntityNotFoundException(nameof(Categoria), id);

        return categoria;
    }

    public async Task<Categoria> CreateAsync(Categoria categoria)
    {
        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));

        // Validar la entidad
        var validationResult = await _validator.ValidateAsync(categoria);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Common.Exceptions.ValidationException(errors);
        }

        categoria.Id = null;
        return await _repository.AddAsync(categoria);
    }

    public async Task<Categoria> UpdateAsync(string id, Categoria categoria)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (categoria == null)
            throw new ArgumentNullException(nameof(categoria));

        // Validar que la entidad existe
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new EntityNotFoundException(nameof(Categoria), id);

        // Validar la entidad antes de actualizar
        var validationResult = await _validator.ValidateAsync(categoria);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Common.Exceptions.ValidationException(errors);
        }

        categoria.Id = id;
        return await _repository.UpdateAsync(categoria);
    }

    public async Task<Categoria> UpdatePartialAsync(string id, Categoria partial)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (partial == null)
            throw new ArgumentNullException(nameof(partial));

        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new EntityNotFoundException(nameof(Categoria), id);

        // Aplicar cambios parciales
        if (!string.IsNullOrEmpty(partial.Nombre))
            existing.Nombre = partial.Nombre;

        if (!string.IsNullOrEmpty(partial.Tipo))
            existing.Tipo = partial.Tipo;

        if (!string.IsNullOrEmpty(partial.UsuarioId))
            existing.UsuarioId = partial.UsuarioId;

        // Validar el objeto actualizado
        var validationResult = await _validator.ValidateAsync(existing);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Common.Exceptions.ValidationException(errors);
        }

        return await _repository.UpdateAsync(existing);
    }

    public async Task<List<Categoria>> GetByUsuarioIdAsync(string usuarioId)
    {
        if (string.IsNullOrWhiteSpace(usuarioId))
            throw new ArgumentNullException(nameof(usuarioId));

        return await _repository.FindAsync(c => c.UsuarioId == usuarioId);
    }

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new EntityNotFoundException(nameof(Categoria), id);
    }
}
