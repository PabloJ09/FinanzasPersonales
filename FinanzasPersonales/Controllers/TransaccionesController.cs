using FinanzasPersonales.Models;
using FinanzasPersonales.Models.DTOs;
using FinanzasPersonales.Services;
using FinanzasPersonales.Common.Results;
using FinanzasPersonales.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

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
    /// Extrae el userId del JWT
    /// </summary>
    private string GetUserId()
    {
        var userId =
            User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ??
            User.FindFirst("usuarioId")?.Value ??
            User.FindFirst("id")?.Value;

        if (string.IsNullOrEmpty(userId))
            throw new UnauthorizedAccessException("UserId not found in token");

        return userId;
    }

    /// <summary>
    /// Obtiene todas las transacciones
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<Transaccion>>> Get()
    {
        var userId = GetUserId();
        var transacciones = await _service.GetAllAsync(userId);
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
        var userId = GetUserId();
        var transaccion = await _service.GetByIdAsync(id, userId);
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
    public async Task<ActionResult<Transaccion>> Create([FromBody] TransaccionCreateDto dto)
    {
        try
        {
            var userId = GetUserId();

            var transaccion = new Transaccion
            {
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                Fecha = dto.Fecha,
                UsuarioId = userId
            };

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
    public async Task<IActionResult> Update(string id, [FromBody] TransaccionUpdateDto dto)
    {
        try
        {
            var userId = GetUserId();
            
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id, userId);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            var transaccion = new Transaccion
            {
                Tipo = dto.Tipo,
                Monto = dto.Monto,
                Descripcion = dto.Descripcion,
                CategoriaId = dto.CategoriaId,
                Fecha = dto.Fecha,
                UsuarioId = userId
            };

            await _service.UpdateAsync(id, transaccion, userId);
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
            var userId = GetUserId();
            await _service.DeleteAsync(id, userId);
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
