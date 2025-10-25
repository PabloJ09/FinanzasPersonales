using FinanzasPersonales.Models;
using FinanzasPersonales.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanzasPersonales.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly UsuarioService _service;

    public UsuariosController(UsuarioService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Usuario usuario)
    {
        try
        {
            await _service.CrearUsuarioAsync(usuario);
            return Ok("Usuario registrado exitosamente");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Usuario usuario)
    {
        bool valid = await _service.ValidarCredencialesAsync(usuario.Email, usuario.PasswordHash);
        if (!valid) return Unauthorized("Credenciales incorrectas");
        return Ok("Login exitoso");
    }
}
