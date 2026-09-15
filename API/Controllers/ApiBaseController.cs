using BLL;
using BLL.CommonResult;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.DTO;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiBaseController : ControllerBase
{
    private IStringLocalizer<Messages>? _localizer;
    protected IStringLocalizer<Messages> Localizer =>
        _localizer ??= HttpContext.RequestServices.GetRequiredService<IStringLocalizer<Messages>>();

    protected IActionResult HandleSuccess<T>(T data, string? messageKey = null, int statusCode = StatusCodes.Status200OK)
    {
        if (statusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        string? message = null;
        if (!string.IsNullOrEmpty(messageKey))
        {
            var localized = Localizer[messageKey];
            message = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
                ? localized.Value
                : messageKey;
        }

        return StatusCode(statusCode, ApiResponse<T>.Ok(data, message, statusCode));
    }

    protected IActionResult HandleMessage(string? messageKey = null, int statusCode = StatusCodes.Status200OK)
    {
        if (statusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        string? message = null;
        if (!string.IsNullOrEmpty(messageKey))
        {
            var localized = Localizer[messageKey];
            message = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
                ? localized.Value
                : messageKey;
        }

        return StatusCode(statusCode, ApiResponse<object>.Ok(null, message, statusCode));
    }

    // Unified HandleRequest: handles Result and Result<T> with optional data, status codes, and message keys
    protected IActionResult HandleRequest(
        Result result, 
        object? data = null, 
        int statusCode = StatusCodes.Status200OK, 
        string? successMessageKey = null, 
        int? errorStatusCode = null)
    {
        if (result.IsFailure)
        {
            return MapErrorsToResponse(result.Errors, errorStatusCode);
        }

        if (statusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        string? message = null;
        if (!string.IsNullOrEmpty(successMessageKey))
        {
            var localized = Localizer[successMessageKey];
            message = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
                ? localized.Value
                : successMessageKey;
        }

        var payload = data ?? result.GetType().GetProperty("Value")?.GetValue(result);

        return StatusCode(statusCode, ApiResponse<object>.Ok(payload, message, statusCode));
    }

    // Generic overload for Result<T>
    protected IActionResult HandleRequest<T>(
        Result<T> result, 
        string? successMessageKey = null, 
        int statusCode = StatusCodes.Status200OK, 
        int? errorStatusCode = null)
    {
        if (result.IsFailure)
        {
            return MapErrorsToResponse(result.Errors, errorStatusCode);
        }

        return HandleSuccess(result.Value, successMessageKey, statusCode);
    }

    // Backward compatibility aliases for HandleResult
    protected IActionResult HandleResult(Result result, string? successMessageKey = null, int statusCode = StatusCodes.Status200OK, int? errorStatusCode = null)
        => HandleRequest(result, successMessageKey: successMessageKey, statusCode: statusCode, errorStatusCode: errorStatusCode);

    protected IActionResult HandleResult<T>(Result<T> result, string? successMessageKey = null, int statusCode = StatusCodes.Status200OK, int? errorStatusCode = null)
        => HandleRequest(result, successMessageKey, statusCode, errorStatusCode);

    protected IActionResult HandleResultWithData<T>(Result result, T? data, string? successMessageKey = null, int statusCode = StatusCodes.Status200OK, int? errorStatusCode = null)
        => HandleRequest(result, data: data, successMessageKey: successMessageKey, statusCode: statusCode, errorStatusCode: errorStatusCode);

    protected IActionResult HandleError(string code, string fallbackDescription, int statusCode = StatusCodes.Status400BadRequest)
    {
        var localized = Localizer[code];
        var desc = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
            ? localized.Value
            : fallbackDescription;

        var errorItem = new ApiErrorItem(code, desc, "Error");
        var response = ApiResponse<object>.Fail(desc, [errorItem], statusCode);
        return StatusCode(statusCode, response);
    }

    private IActionResult MapErrorsToResponse(IReadOnlyList<Error> errors, int? overrideStatusCode = null)
    {
        var errorItems = new List<ApiErrorItem>();

        foreach (var error in errors)
        {
            var localized = Localizer[error.MessageKey];
            var desc = (!localized.ResourceNotFound && !string.IsNullOrEmpty(localized.Value))
                ? localized.Value
                : error.Description;

            errorItems.Add(new ApiErrorItem(error.Code, desc, error.Type.ToString()));
        }

        int statusCode;
        if (overrideStatusCode.HasValue)
        {
            statusCode = overrideStatusCode.Value;
        }
        else
        {
            var firstError = errors.FirstOrDefault();
            statusCode = firstError?.Type switch
            {
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
                ErrorType.Forbidden => StatusCodes.Status403Forbidden,
                ErrorType.InvalidCredentials => StatusCodes.Status401Unauthorized,
                ErrorType.Locked => StatusCodes.Status423Locked,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status400BadRequest
            };
        }

        string topLevelMessage = string.Join(" | ", errorItems.Select(e => e.Description));
        var response = ApiResponse<object>.Fail(topLevelMessage, errorItems, statusCode);
        return StatusCode(statusCode, response);
    }
}
