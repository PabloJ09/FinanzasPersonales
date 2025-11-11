using System.Net;
using System.Text.Json;
using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Common.Results;

namespace FinanzasPersonales.Middleware;

/// <summary>
/// Middleware global para manejar excepciones.
/// Principio: Single Responsibility - Solo captura y formatea excepciones
/// Principio: Dependency Inversion - Usa abstracciones para logging
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new ApiResponse();

        switch (exception)
        {
            case EntityNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = ex.Code
                };
                break;

            case ValidationException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = ex.Code,
                    Errors = ex.Errors
                };
                break;

            case UnauthorizedException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = ex.Code
                };
                break;

            case DomainException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = ex.Code,
                    Errors = ex.Errors
                };
                break;

            case KeyNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = "ENTITY_NOT_FOUND"
                };
                break;

            case ArgumentException ex:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response = new ApiResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Code = "INVALID_ARGUMENT"
                };
                break;

            default:
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response = new ApiResponse
                {
                    Success = false,
                    Message = "Ha ocurrido un error interno. Por favor, intenta más tarde.",
                    Code = "INTERNAL_SERVER_ERROR"
                };
                break;
        }

        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extensión para registrar el middleware de manejo de excepciones
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
