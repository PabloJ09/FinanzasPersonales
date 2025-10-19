using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Models;
using FinanzasPersonales.Services;

namespace FinanzasPersonales.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionesController : ControllerBase
    {
        private readonly TransaccionService _service;

        public TransaccionesController()
        {
            _service = new TransaccionService();
        }

        [HttpGet]
        public async Task<ActionResult<List<Transaccion>>> Get() =>
            Ok(await _service.GetAsync());

        [HttpPost]
        public async Task<ActionResult<Transaccion>> Post([FromBody] Transaccion transaccion)
        {
            var nueva = await _service.CreateAsync(transaccion);
            return CreatedAtAction(nameof(Get), new { id = nueva.Id }, nueva);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
