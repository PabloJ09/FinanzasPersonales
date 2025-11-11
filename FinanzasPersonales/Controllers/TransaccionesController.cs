using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.Common.Results;
using FinanzasPersonales.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanzasPersonales.Controllers;

/// <summary>
/// Controlador de Transacciones.
/// Principio: Single Responsibility - Solo maneja HTTP
/// Principio: Dependency Inversion - Depende de ITransaccionService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransaccionesController : ControllerBase
{
    private readonly ITransaccionService _service;

    public TransaccionesController(ITransaccionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Obtiene todas las transacciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<Transaccion>>> Get()
    {
        var transacciones = await _service.GetAllAsync();
        return Ok(transacciones);
    }

    /// <summary>
    /// Obtiene una transacción por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Transaccion>> GetById(string id)
    {
        var transaccion = await _service.GetByIdAsync(id);
        if (transaccion == null)
            return NotFound(new { Message = "Not found" });
        return Ok(transaccion);
    }

    /// <summary>
    /// Crea una nueva transacción
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Transaccion>> Create([FromBody] Transaccion transaccion)
    {
        try
        {
            var creada = await _service.CreateAsync(transaccion);
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, creada);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Actualiza completamente una transacción (PUT)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string id, [FromBody] Transaccion transaccion)
    {
        try
        {
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            await _service.UpdateAsync(id, transaccion);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (System.ComponentModel.DataAnnotations.ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Elimina una transacción
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
