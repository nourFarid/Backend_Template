using BLL.CommonResult;
using Shared.DTO.AuthDtos;

namespace BLL.Services;

public record AuthResult(string Token, string RefreshToken, AuthResponseDto Response);

public interface IAuthService
{
    Task<Result<AuthResult>> RegisterAsync(RegisterDto dto);
    Task<Result<AuthResult>> LoginAsync(LoginDto dto);
    Task<Result> LogoutAsync(string userId);
    Task<bool> RoleExistsAsync(string roleName);
    Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto passDto);
    Task<Result> ResetPasswordAsync(string userId, string password);
    Task<Result<AuthResult>> RefreshTokenAsync(RefreshTokenRequestDto dto);
}
