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
    /// Obtiene todas las categorías del usuario autenticado
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<Categoria>>> Get()
    {
        var userId = GetUserId();
        var categorias = await _service.GetAllAsync(userId);
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
        var userId = GetUserId();
        var categoria = await _service.GetByIdAsync(id, userId);
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
    public async Task<ActionResult<Categoria>> Create([FromBody] CategoriaCreateDto dto)
    {
        try
        {
            var userId = GetUserId();

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                UsuarioId = userId
            };

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
    public async Task<IActionResult> Update(string id, [FromBody] CategoriaUpdateDto dto)
    {
        try
        {
            var userId = GetUserId();
            
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id, userId);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            var categoria = new Categoria
            {
                Nombre = dto.Nombre,
                Tipo = dto.Tipo,
                UsuarioId = userId
            };

            await _service.UpdateAsync(id, categoria, userId);
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
    public async Task<ActionResult<Categoria>> UpdatePartial(string id, [FromBody] CategoriaPartialUpdateDto dto)
    {
        try
        {
            var userId = GetUserId();
            
            // Ensure existence first so controller tests receive NotFound before validation errors
            var existing = await _service.GetByIdAsync(id, userId);
            if (existing == null)
                return NotFound(new { Message = "Not found" });

            var partial = new Categoria
            {
                Nombre = dto.Nombre ?? string.Empty,
                Tipo = dto.Tipo ?? string.Empty,
                UsuarioId = userId
            };

            var actualizada = await _service.UpdatePartialAsync(id, partial, userId);
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
