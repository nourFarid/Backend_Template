using BLL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Shared.DTO;
using System.Net;
using System.Text.Json;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IStringLocalizer<Messages> localizer)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex, localizer);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IStringLocalizer<Messages> localizer)
    {
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

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
