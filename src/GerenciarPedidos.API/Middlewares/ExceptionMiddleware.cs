using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace GerenciarPedidos.API.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Erro de regra de negocio: {Message}", ex.Message);
            await HandleExceptionAsync(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado na API");
            await HandleExceptionAsync(context, HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado");
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, HttpStatusCode statusCode, string message)
    {
        var response = new { erro = message };
        var json = JsonSerializer.Serialize(response);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        return context.Response.WriteAsync(json);
    }
}
