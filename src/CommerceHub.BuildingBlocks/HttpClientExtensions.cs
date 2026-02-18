using System.Net;
using Microsoft.AspNetCore.Http;

namespace CommerceHub.BuildingBlocks;

/// <summary>
/// Extensions for forwarding error responses (RFC 7807 ProblemDetails) from backend services.
/// </summary>
public static class HttpClientExtensions
{
    /// <summary>
    /// Forwards a non-success HTTP response as an IResult, preserving ProblemDetails when present.
    /// When the response is application/problem+json or application/json, returns the body as-is
    /// so the client receives consistent RFC 7807 format.
    /// </summary>
    public static async Task<IResult> ForwardErrorResponseAsync(this HttpResponseMessage response, CancellationToken ct = default)
    {
        var statusCode = (int)response.StatusCode;
        var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/json";
        var body = await response.Content.ReadAsStringAsync(ct);

        // Forward ProblemDetails responses as-is (application/problem+json or application/json with ProblemDetails structure)
        if (contentType.Contains("problem+json", StringComparison.OrdinalIgnoreCase) ||
            (contentType.Contains("json", StringComparison.OrdinalIgnoreCase) && IsLikelyProblemDetails(body)))
        {
            return Results.Content(body, contentType, statusCode: statusCode);
        }

        // Fallback: wrap in ProblemDetails when response is not already structured
        return Results.Problem(
            statusCode: statusCode,
            title: GetDefaultTitle(statusCode),
            detail: body.Length > 0 ? body : null);
    }

    private static bool IsLikelyProblemDetails(string body)
    {
        if (string.IsNullOrWhiteSpace(body) || body.Length < 20) return false;
        return body.Contains("\"status\"", StringComparison.Ordinal) &&
               (body.Contains("\"title\"", StringComparison.Ordinal) || body.Contains("\"detail\"", StringComparison.Ordinal));
    }

    private static string GetDefaultTitle(int statusCode) => statusCode switch
    {
        (int)HttpStatusCode.BadRequest => "Bad Request",
        (int)HttpStatusCode.Unauthorized => "Unauthorized",
        (int)HttpStatusCode.Forbidden => "Forbidden",
        (int)HttpStatusCode.NotFound => "Not Found",
        (int)HttpStatusCode.Conflict => "Conflict",
        (int)HttpStatusCode.UnprocessableEntity => "Unprocessable Entity",
        _ => "An error occurred"
    };
}
