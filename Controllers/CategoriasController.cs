using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CategoriaService _service;

    public CategoriasController(CategoriaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<List<Categoria>> Get() => await _service.GetAsync();

    [HttpPost]
    public async Task<Categoria> Create([FromBody] Categoria categoria) => await _service.CreateAsync(categoria);

    [HttpDelete("{id}")]
    public async Task Delete(string id) => await _service.DeleteAsync(id);
}
