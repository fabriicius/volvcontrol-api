using System.Text.Json;
using Microsoft.AspNetCore.Http;
using volvcontrol_api.domain.Exceptions;

namespace volvcontrol_api.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await WriteErrorResponseAsync(context, ex);
        }
    }

    private async Task WriteErrorResponseAsync(HttpContext context, Exception ex)
    {
        var (statusCode, errorCode, message) = MapException(ex);

        var response = new
        {
            message,
            errorCode,
            details = _environment.IsDevelopment() ? ex.Message : null
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static (int StatusCode, string ErrorCode, string Message) MapException(Exception ex)
    {
        return ex switch
        {
            ConflictException => (StatusCodes.Status409Conflict, "CONFLICT", ex.Message),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "NOT_FOUND", "Recurso não encontrado."),
            ArgumentException => (StatusCodes.Status400BadRequest, "BAD_REQUEST", "Dados inválidos na requisição."),
            FormatException => (StatusCodes.Status400BadRequest, "BAD_REQUEST", "Formato de dados inválido."),
            InvalidOperationException invalidOp when invalidOp.Message.Contains("Database error", StringComparison.OrdinalIgnoreCase)
                => (StatusCodes.Status500InternalServerError, "DATABASE_ERROR", "Ocorreu um erro ao acessar o banco de dados."),
            _ => (StatusCodes.Status500InternalServerError, "INTERNAL_ERROR", "Ocorreu um erro interno ao processar a solicitação.")
        };
    }
}
