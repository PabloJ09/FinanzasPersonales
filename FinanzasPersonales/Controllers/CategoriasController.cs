using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanzasPersonales.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _service;

    public CategoriasController(CategoriaService service)
    {
        _service = service;
    }

    // GET: api/categorias
    [HttpGet]
    public async Task<ActionResult<List<Categoria>>> Get()
        => Ok(await _service.GetAsync());

    // GET: api/categorias/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Categoria>> GetById(string id)
    {
        var categoria = await _service.GetByIdAsync(id);
        if (categoria == null) return NotFound("Categoría no encontrada");
        return Ok(categoria);
    }

    // POST: api/categorias
    [HttpPost]
    public async Task<ActionResult<Categoria>> Create([FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _service.CreateAsync(categoria);
        return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
    }

    // PUT: api/categorias/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Categoria categoria)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existente = await _service.GetByIdAsync(id);
        if (existente == null) return NotFound("Categoría no encontrada");

        await _service.UpdateAsync(id, categoria);
        return NoContent();
    }

    // DELETE: api/categorias/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existente = await _service.GetByIdAsync(id);
        if (existente == null) return NotFound("Categoría no encontrada");

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
