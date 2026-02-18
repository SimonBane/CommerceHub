using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;

namespace CommerceHub.BuildingBlocks.Middleware;

/// <summary>
/// Global exception handler producing RFC 7807 ProblemDetails consistently across all services.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment) : IExceptionHandler
{
    private const string TypeBase = "https://tools.ietf.org/html/rfc7231";

    public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (statusCode, title, type) = GetStatusCodeAndTitle(exception);

        // In production, avoid exposing internal exception details for 500 errors
        var detail = statusCode == StatusCodes.Status500InternalServerError && !environment.IsDevelopment()
            ? "An error occurred while processing your request."
            : exception.Message;

        return problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = type,
                Status = statusCode,
                Title = title,
                Detail = detail
            }
        });
    }

    private static (int StatusCode, string Title, string Type) GetStatusCodeAndTitle(Exception exception) => exception switch
    {
        UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", $"{TypeBase}#section-6.5.3"),
        ArgumentException or ArgumentNullException => (StatusCodes.Status400BadRequest, "Bad Request", $"{TypeBase}#section-6.5.1"),
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", $"{TypeBase}#section-6.5.4"),
        InvalidOperationException => (StatusCodes.Status400BadRequest, "Bad Request", $"{TypeBase}#section-6.5.1"),
        _ => (StatusCodes.Status500InternalServerError, "An error occurred while processing your request.", $"{TypeBase}#section-6.6.1")
    };
}
