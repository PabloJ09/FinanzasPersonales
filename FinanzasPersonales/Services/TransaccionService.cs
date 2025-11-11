using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using FluentValidation;

namespace FinanzasPersonales.Services;

/// <summary>
/// Servicio de dominio para Transacciones.
/// Principio: Single Responsibility - Solo lógica de transacciones
/// Principio: Dependency Inversion - Depende de IRepository, no de MongoDB directamente
/// </summary>
public interface ITransaccionService
{
    Task<List<Transaccion>> GetAllAsync();
    Task<Transaccion> GetByIdAsync(string id);
    Task<Transaccion> CreateAsync(Transaccion transaccion);
    Task<Transaccion> UpdateAsync(string id, Transaccion transaccion);
    Task<List<Transaccion>> GetByUsuarioIdAsync(string usuarioId);
    Task<List<Transaccion>> GetByCategoriaAsync(string categoriaId);
    Task DeleteAsync(string id);
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

    public async Task<List<Transaccion>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Transaccion> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var transaccion = await _repository.GetByIdAsync(id);
        if (transaccion == null)
            throw new EntityNotFoundException(nameof(Transaccion), id);

        return transaccion;
    }

    public async Task<Transaccion> CreateAsync(Transaccion transaccion)
    {
        if (transaccion == null)
            throw new ArgumentNullException(nameof(transaccion));

        // Validar la entidad
        var validationResult = await _validator.ValidateAsync(transaccion);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Common.Exceptions.ValidationException(errors);
        }

        transaccion.Id = null;
        return await _repository.AddAsync(transaccion);
    }

    public async Task<Transaccion> UpdateAsync(string id, Transaccion transaccion)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        if (transaccion == null)
            throw new ArgumentNullException(nameof(transaccion));

        // Validar que la entidad existe
        var existing = await _repository.GetByIdAsync(id);
        if (existing == null)
            throw new EntityNotFoundException(nameof(Transaccion), id);

        // Validar la entidad antes de actualizar
        var validationResult = await _validator.ValidateAsync(transaccion);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Common.Exceptions.ValidationException(errors);
        }

        transaccion.Id = id;
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

    public async Task DeleteAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));

        var deleted = await _repository.DeleteAsync(id);
        if (!deleted)
            throw new EntityNotFoundException(nameof(Transaccion), id);
    }
}
