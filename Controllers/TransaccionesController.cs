using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly TransaccionService _service;

    public TransaccionesController(TransaccionService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<List<Transaccion>> Get() => await _service.GetAsync();

    [HttpPost]
    public async Task<Transaccion> Create([FromBody] Transaccion transaccion) => await _service.CreateAsync(transaccion);

    [HttpDelete("{id}")]
    public async Task Delete(string id) => await _service.DeleteAsync(id);
}
