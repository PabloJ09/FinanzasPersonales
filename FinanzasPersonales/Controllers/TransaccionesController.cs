using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FinanzasPersonales.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransaccionesController : ControllerBase
{
    private readonly TransaccionService _service;

    public TransaccionesController(TransaccionService service)
    {
        _service = service;
    }

    // GET: api/transacciones
    [HttpGet]
    public async Task<ActionResult<List<Transaccion>>> Get()
        => Ok(await _service.GetAsync());

    // GET: api/transacciones/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<Transaccion>> GetById(string id)
    {
        var transaccion = await _service.GetByIdAsync(id);
        if (transaccion == null) return NotFound("Transacción no encontrada");
        return Ok(transaccion);
    }

    // POST: api/transacciones
    [HttpPost]
    public async Task<ActionResult<Transaccion>> Create([FromBody] Transaccion transaccion)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _service.CreateAsync(transaccion);
        return CreatedAtAction(nameof(GetById), new { id = transaccion.Id }, transaccion);
    }

    // PUT: api/transacciones/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Transaccion transaccion)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var existente = await _service.GetByIdAsync(id);
        if (existente == null) return NotFound("Transacción no encontrada");

        await _service.UpdateAsync(id, transaccion);
        return NoContent();
    }

    // DELETE: api/transacciones/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var existente = await _service.GetByIdAsync(id);
        if (existente == null) return NotFound("Transacción no encontrada");

        await _service.DeleteAsync(id);
        return NoContent();
    }
}
