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
    public async Task<ActionResult<ApiResponse<List<Transaccion>>>> Get()
    {
        try
        {
            var transacciones = await _service.GetAllAsync();
            var response = new ApiResponse<List<Transaccion>>
            {
                Success = true,
                Data = transacciones,
                Message = "Transacciones obtenidas exitosamente"
            };
            return Ok(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse<List<Transaccion>>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
        }
    }

    /// <summary>
    /// Obtiene una transacción por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Transaccion>>> GetById(string id)
    {
        try
        {
            var transaccion = await _service.GetByIdAsync(id);
            var response = new ApiResponse<Transaccion>
            {
                Success = true,
                Data = transaccion,
                Message = "Transacción obtenida exitosamente"
            };
            return Ok(response);
        }
        catch (EntityNotFoundException ex)
        {
            var response = new ApiResponse<Transaccion>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code
            };
            return NotFound(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse<Transaccion>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
        }
    }

    /// <summary>
    /// Crea una nueva transacción
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Transaccion>>> Create([FromBody] Transaccion transaccion)
    {
        try
        {
            var creada = await _service.CreateAsync(transaccion);
            var response = new ApiResponse<Transaccion>
            {
                Success = true,
                Data = creada,
                Message = "Transacción creada exitosamente"
            };
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, response);
        }
        catch (ValidationException ex)
        {
            var response = new ApiResponse<Transaccion>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse<Transaccion>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
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
            await _service.UpdateAsync(id, transaccion);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code
            };
            return NotFound(response);
        }
        catch (ValidationException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
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
        catch (EntityNotFoundException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code
            };
            return NotFound(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code,
                Errors = ex.Errors
            };
            return BadRequest(response);
        }
    }
}
