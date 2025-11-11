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
    public async Task<ActionResult<List<Categoria>>> Get()
    {
        var categorias = await _service.GetAllAsync();
        return Ok(categorias);
    }

    /// <summary>
    /// Obtiene una categoría por su ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Categoria>> GetById(string id)
    {
        var categoria = await _service.GetByIdAsync(id);
        if (categoria == null)
            return NotFound(new { Message = "Not found" });
        return Ok(categoria);
    }

    /// <summary>
    /// Crea una nueva categoría
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Categoria>> Create([FromBody] Categoria categoria)
    {
        try
        {
            var creada = await _service.CreateAsync(categoria);
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
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            await _service.UpdateAsync(id, categoria);
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
    /// Actualiza parcialmente una categoría (PATCH)
    /// </summary>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<Categoria>> UpdatePartial(string id, [FromBody] Categoria categoria)
    {
        try
        {
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            var actualizada = await _service.UpdatePartialAsync(id, categoria);
            return Ok(actualizada);
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
