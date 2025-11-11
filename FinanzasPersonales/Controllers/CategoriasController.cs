using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using FinanzasPersonales.Common.Results;
using FinanzasPersonales.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanzasPersonales.Controllers;

/// <summary>
/// Controlador de Categorías.
/// Principio: Single Responsibility - Solo maneja HTTP
/// Principio: Dependency Inversion - Depende de ICategoriaService
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICategoriaService _service;

    public CategoriasController(ICategoriaService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    /// <summary>
    /// Obtiene todas las categorías del usuario autenticado
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<List<Categoria>>>> Get()
    {
        try
        {
            var categorias = await _service.GetAllAsync();
            var response = new ApiResponse<List<Categoria>>
            {
                Success = true,
                Data = categorias,
                Message = "Categorías obtenidas exitosamente"
            };
            return Ok(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse<List<Categoria>>
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
    /// Obtiene una categoría por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Categoria>>> GetById(string id)
    {
        try
        {
            var categoria = await _service.GetByIdAsync(id);
            var response = new ApiResponse<Categoria>
            {
                Success = true,
                Data = categoria,
                Message = "Categoría obtenida exitosamente"
            };
            return Ok(response);
        }
        catch (EntityNotFoundException ex)
        {
            var response = new ApiResponse<Categoria>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code
            };
            return NotFound(response);
        }
        catch (DomainException ex)
        {
            var response = new ApiResponse<Categoria>
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
    /// Crea una nueva categoría
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Categoria>>> Create([FromBody] Categoria categoria)
    {
        try
        {
            var creada = await _service.CreateAsync(categoria);
            var response = new ApiResponse<Categoria>
            {
                Success = true,
                Data = creada,
                Message = "Categoría creada exitosamente"
            };
            return CreatedAtAction(nameof(GetById), new { id = creada.Id }, response);
        }
        catch (ValidationException ex)
        {
            var response = new ApiResponse<Categoria>
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
            var response = new ApiResponse<Categoria>
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
    /// Actualiza completamente una categoría (PUT)
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update(string id, [FromBody] Categoria categoria)
    {
        try
        {
            await _service.UpdateAsync(id, categoria);
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
    /// Actualiza parcialmente una categoría (PATCH)
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<Categoria>>> UpdatePartial(string id, [FromBody] Categoria categoria)
    {
        try
        {
            var actualizada = await _service.UpdatePartialAsync(id, categoria);
            var response = new ApiResponse<Categoria>
            {
                Success = true,
                Data = actualizada,
                Message = "Categoría actualizada exitosamente"
            };
            return Ok(response);
        }
        catch (EntityNotFoundException ex)
        {
            var response = new ApiResponse<Categoria>
            {
                Success = false,
                Message = ex.Message,
                Code = ex.Code
            };
            return NotFound(response);
        }
        catch (ValidationException ex)
        {
            var response = new ApiResponse<Categoria>
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
            var response = new ApiResponse<Categoria>
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
    /// Elimina una categoría
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
