using BLL.CommonResult;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.DTO.AuthDtos;
using System.Security.Claims;

namespace API.Controllers;

[EnableRateLimiting("AuthLimiter")]
public class AuthController : ApiBaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    private void SetTokenCookies(string? token, string? refreshToken, bool rememberMe = false)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/"
        };

        if (rememberMe)
        {
            cookieOptions.Expires = DateTime.UtcNow.AddDays(7);
        }

        if (!string.IsNullOrEmpty(token))
        {
            Response.Cookies.Append("jwt", token, cookieOptions);
        }
        if (!string.IsNullOrEmpty(refreshToken))
        {
            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }
    }

    private void ClearTokenCookies()
    {
        var expiredOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(-1)
        };
        Response.Cookies.Append("jwt", string.Empty, expiredOptions);
        Response.Cookies.Append("refreshToken", string.Empty, expiredOptions);
    }

    [HttpPost("Register")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        if (result.IsFailure)
        {
            return HandleRequest(result);
        }

        return HandleRequest(result, data: result.Value!.Response, successMessageKey: "Auth.UserAddedSuccess", statusCode: StatusCodes.Status201Created);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result.IsFailure)
        {
            return HandleRequest(result);
        }

        var authResult = result.Value!;
        if (authResult.Response.NewUser)
        {
            SetTokenCookies(authResult.Token, string.Empty);
            return HandleRequest(result, data: authResult.Response, successMessageKey: "Auth.PasswordChangeRequired");
        }

        SetTokenCookies(authResult.Token, authResult.RefreshToken, dto.RememberMe);
        return HandleRequest(result, data: authResult.Response);
    }

    [HttpPost("CheckRole")]
    [Authorize]
    public IActionResult CheckRole([FromBody] string role)
    {
        if (string.IsNullOrEmpty(role))
        {
            return HandleError("Auth.RoleRequired", "Role must be provided to check.", StatusCodes.Status400BadRequest);
        }

        bool hasRole = User.IsInRole(role);
        if (!hasRole)
        {
            return HandleError("Auth.RoleForbidden", "You do not have the required role.", StatusCodes.Status403Forbidden);
        }

        return HandleMessage("Auth.RoleAllowed");
    }

    [HttpPost("UpdatePassword")]
    [Authorize]
    public async Task<IActionResult> UpdatePassword([FromBody] ChangePasswordDto formBody)
    {
        if (string.IsNullOrEmpty(formBody.NewPassword) || string.IsNullOrEmpty(formBody.ConfirmPassword))
        {
            return HandleError("Auth.PasswordAndConfirmRequired", "Password and confirmation are required.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrEmpty(formBody.CurrentPassword))
        {
            return HandleError("Auth.CurrentPasswordRequired", "Current password is required.", StatusCodes.Status400BadRequest);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return HandleError("Auth.UserNotFound", "User not found.", StatusCodes.Status404NotFound);
        }

        var result = await _authService.ChangePasswordAsync(userId, formBody);
        if (result.IsFailure)
        {
            return HandleRequest(result);
        }

        await _authService.LogoutAsync(userId);
        ClearTokenCookies();

        return HandleRequest(result, successMessageKey: "Auth.PasswordChangeSuccess");
    }

    [HttpPost("Logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userId))
        {
            await _authService.LogoutAsync(userId);
        }

        ClearTokenCookies();
        return HandleRequest(Result.Success(), successMessageKey: "Auth.LogoutSuccess");
    }

    [HttpPost("ResetPassword/{id}")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> ResetPassword([FromBody] ChangePasswordDto dto, string id)
    {
        var result = await _authService.ResetPasswordAsync(id, dto.NewPassword);
        return HandleRequest(result, successMessageKey: "Auth.PasswordResetSuccess");
    }

    [HttpGet("ReAssign")]
    [Authorize]
    public async Task<IActionResult> ReAssign()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return HandleError("Auth.UserNotFound", "User does not exist.", StatusCodes.Status404NotFound);
        }

        var result = await _authService.ReGenerateTokenAsync(userId);
        if (result.IsFailure)
        {
            return HandleRequest(result);
        }

        SetTokenCookies(result.Value, null);
        return HandleMessage("Auth.TokenReassigned");
    }

    [HttpPost("RefreshToken")]
    public async Task<IActionResult> RefreshToken()
    {
        var expiredToken = Request.Cookies["jwt"];
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrEmpty(expiredToken) || string.IsNullOrEmpty(refreshToken))
        {
            return HandleError("Auth.TokensMissing", "Tokens are missing from cookies.", StatusCodes.Status401Unauthorized);
        }

        var dto = new RefreshTokenRequestDto
        {
            ExpiredToken = expiredToken,
            RefreshToken = refreshToken
        };

        var result = await _authService.RefreshTokenAsync(dto);
        if (result.IsFailure)
        {
            return HandleRequest(result);
        }

        SetTokenCookies(result.Value!.Token, result.Value.RefreshToken);
        return HandleRequest(result, data: result.Value.Response);
    }
}
