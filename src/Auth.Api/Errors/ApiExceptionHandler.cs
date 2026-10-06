using Microsoft.AspNetCore.Diagnostics;
using Auth.Application.Exceptions;

namespace Auth.Api.Errors;

public sealed class ApiExceptionHandler(ILogger<ApiExceptionHandler> logger) : IExceptionHandler
{
    private static readonly Action<ILogger, int, Exception?> LogRequestFailure =
        LoggerMessage.Define<int>(LogLevel.Error, new EventId(1, "RequestFailure"),
            "Request failed with status code {StatusCode}");

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, error) = exception switch
        {
            ArgumentException argumentException => (StatusCodes.Status400BadRequest,
                new ApiError("validation_error", argumentException.Message)),
            ConflictException conflictException => (StatusCodes.Status409Conflict,
                new ApiError("conflict", conflictException.Message)),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized,
                new ApiError("invalid_credentials", "Credenciais inválidas.")),
            KeyNotFoundException => (StatusCodes.Status404NotFound,
                new ApiError("not_found", "Recurso não encontrado.")),
            UnauthorizedAccessException => (StatusCodes.Status403Forbidden,
                new ApiError("forbidden", "Acesso não permitido.")),
            _ => (StatusCodes.Status500InternalServerError,
                new ApiError("internal_error", "Ocorreu um erro inesperado."))
        };

        LogRequestFailure(logger, statusCode, exception);
        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(error, cancellationToken);
        return true;
    }
}
