using Microsoft.AspNetCore.Mvc;
using FinanzasPersonales.Common.Results;
using FinanzasPersonales.Models.DTOs.IA;
using FinanzasPersonales.Services.IA;
using FinanzasPersonales.Common.Exceptions;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace FinanzasPersonales.Controllers;

/// <summary>
/// Controlador para procesamiento de transacciones usando IA
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IAController : ControllerBase
{
    private readonly IProcesamientoIAService _procesamientoIAService;
    private readonly ILogger<IAController> _logger;

    public IAController(
        IProcesamientoIAService procesamientoIAService,
        ILogger<IAController> logger)
    {
        _procesamientoIAService = procesamientoIAService;
        _logger = logger;
    }

    /// <summary>
    /// Extrae el userId del JWT para asociar todo a la sesión autenticada
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
    /// Procesa texto libre y devuelve datos estructurados para revisión
    /// </summary>
    /// <param name="request">Texto libre del usuario</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Datos estructurados (Categoría y Transacción) para revisión del usuario</returns>
    [HttpPost("procesar-texto")]
    [ProducesResponseType(typeof(ApiResponse<RespuestaIADto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<RespuestaIADto>>> ProcesarTexto(
        [FromBody] TextoLibreRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            request.UsuarioId = userId; // siempre forzamos el userId autenticado

            _logger.LogInformation("Procesando texto libre: {Texto} para usuario {UsuarioId}", request.Texto, userId);

            var resultado = await _procesamientoIAService.ProcesarTextoLibreAsync(request, cancellationToken);

            return Ok(new ApiResponse<RespuestaIADto>
            {
                Success = true,
                Data = resultado,
                Message = "Texto procesado exitosamente. Por favor, revisa los datos antes de confirmar."
            });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Solicitud inválida al procesar texto libre");
            return BadRequest(new ApiResponse<RespuestaIADto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error operativo al procesar texto libre");
            return StatusCode(502, new ApiResponse<RespuestaIADto>
            {
                Success = false,
                Message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar texto libre");
            return StatusCode(500, new ApiResponse<RespuestaIADto>
            {
                Success = false,
                Message = "Error al procesar el texto. Por favor, intenta nuevamente."
            });
        }
    }

    /// <summary>
    /// Confirma y guarda la transacción procesada por IA en la base de datos
    /// </summary>
    /// <param name="request">Datos confirmados de categoría y transacción</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Categoría y transacción guardadas</returns>
    [HttpPost("confirmar-transaccion")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ApiResponse<object>>> ConfirmarTransaccion(
        [FromBody] ConfirmarTransaccionDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var userId = GetUserId();
            request.UsuarioId = userId; // asegurar consistencia con la sesión
            if (request.Categoria != null)
            {
                request.Categoria.UsuarioId = userId;
            }
            if (request.Transaccion != null)
            {
                request.Transaccion.UsuarioId = userId;
            }

            _logger.LogInformation("Confirmando transacción para usuario {UsuarioId}", userId);

            var (categoria, transaccion) = await _procesamientoIAService.ConfirmarYGuardarAsync(request, cancellationToken);

            var resultado = new
            {
                Categoria = categoria,
                Transaccion = transaccion
            };

            return StatusCode(201, new ApiResponse<object>
            {
                Success = true,
                Data = resultado,
                Message = "Transacción guardada exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar transacción");
            return StatusCode(500, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al guardar la transacción. Por favor, intenta nuevamente."
            });
        }
    }

    /// <summary>
    /// Endpoint de salud para verificar la configuración de IA
    /// </summary>
    /// <returns>Estado de configuración</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public ActionResult<ApiResponse<object>> Health()
    {
        try
        {
            var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
            
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return StatusCode(503, new ApiResponse<object>
                {
                    Success = false,
                    Message = "ANTHROPIC_API_KEY no está configurada"
                });
            }

            var modelo = Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") ?? "claude-3-5-sonnet-20241022";

            var estado = new
            {
                IAConfigurada = true,
                Proveedor = "Anthropic",
                Modelo = modelo
            };

            return Ok(new ApiResponse<object>
            {
                Success = true,
                Data = estado,
                Message = "Servicio de IA configurado correctamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar configuración de IA");
            return StatusCode(503, new ApiResponse<object>
            {
                Success = false,
                Message = "Error al verificar configuración de IA"
            });
        }
    }
}
