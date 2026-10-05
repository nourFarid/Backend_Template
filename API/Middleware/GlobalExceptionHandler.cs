using BLL;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.DTO;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IStringLocalizer<Messages> localizer) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1. Client disconnection - not an internal server error, skip noisy logging
        if (exception is OperationCanceledException)
        {
            logger.LogInformation("Request cancelled by client at {Path}", httpContext.Request.Path);
            httpContext.Response.StatusCode = 499; // Client Closed Request
            return true;
        }

        // 2. Prevent stream collision if response headers have already started flushing
        if (httpContext.Response.HasStarted)
        {
            logger.LogWarning("The response has already started, cannot write error response for: {Message}", exception.Message);
            return false;
        }

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var statusCode = HttpStatusCode.InternalServerError;
        string errorCode = "General.UnhandledException";
        string defaultMessage = "An unexpected error occurred. Please try again later.";

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                errorCode = "General.Unauthorized";
                defaultMessage = "Unauthorized access.";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                errorCode = "General.NotFound";
                defaultMessage = "The requested resource was not found.";
                break;

            case ArgumentException:
            case InvalidOperationException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "General.Validation";
                defaultMessage = exception.Message;
                break;

            case DbUpdateConcurrencyException:
                statusCode = HttpStatusCode.Conflict;
                errorCode = "General.Conflict";
                defaultMessage = "A conflict occurred while updating the database.";
                break;

            case DbUpdateException:
                statusCode = HttpStatusCode.BadRequest;
                errorCode = "General.DatabaseUpdateFailed";
                defaultMessage = "Database operation failed.";
                break;
        }

        var localized = localizer[errorCode];
        var resolvedMessage = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
            ? localized.Value
            : defaultMessage;

        var errorItem = new ApiErrorItem(errorCode, resolvedMessage, "ServerCrash");
        var response = ApiResponse<object>.Fail(resolvedMessage, [errorItem], (int)statusCode);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)statusCode;

        await httpContext.Response.WriteAsJsonAsync(response, JsonOptions, cancellationToken);
        return true;
    }
}
