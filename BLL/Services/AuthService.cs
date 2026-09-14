using BLL.CommonResult;
using BLL.JWT;
using DAL.Interfaces;
using DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.DTO.AuthDtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BLL.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly JwtAuthOption _jwtOptions;
    private readonly IUserRepository _userRepository;

    public AuthService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole> roleManager,
        IOptions<JwtAuthOption> jwtOptions,
        IUserRepository userRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _jwtOptions = jwtOptions.Value;
        _userRepository = userRepository;
    }

    public async Task<Result<AuthResult>> RegisterAsync(RegisterDto dto)
    {
        var user = new User
        {
            UserName = dto.UserName,
            Email = dto.Email,
            FullName = dto.FullName ?? string.Empty,
            UserCode = dto.UserCode ?? string.Empty
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            return Error.Validation("Auth.RegisterError", string.Join(" | ", result.Errors.Select(e => e.Description)));
        }

        await _userManager.AddToRoleAsync(user, dto.Role);
        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);

        var response = new AuthResponseDto
        {
            Role = roles.FirstOrDefault(),
            NewUser = user.NewUser,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName,
            Email = user.Email,
            UserCode = user.UserCode,
            UserId = user.Id
        };

        return new AuthResult(token, string.Empty, response);
    }

    public async Task<Result<AuthResult>> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.UserName) 
                   ?? await _userManager.FindByNameAsync(dto.UserName);

        if (user == null)
            return Error.InvalidCredentials("Auth.InvalidCredentials", "Invalid username or password.");

        if (!user.IsActive)
            return Error.Unauthorized("Auth.UserDeactivated", "Account is deactivated. Please contact support.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, lockoutOnFailure: true);
        if (result.IsLockedOut)
        {
            return Error.Locked("Auth.AccountLocked", "Account is locked due to multiple failed login attempts. Please try again later.");
        }

        if (!result.Succeeded)
        {
            return Error.InvalidCredentials("Auth.InvalidCredentials", "Invalid username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        string accessToken = GenerateJwtToken(user, roles);
        string refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponseDto
        {
            Role = roles.FirstOrDefault(),
            NewUser = user.NewUser,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName
        };

        return new AuthResult(accessToken, refreshToken, authResponse);
    }

    public async Task<Result> LogoutAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            await _userManager.UpdateSecurityStampAsync(user);
        }

        return Result.Success();
    }

    private string GenerateJwtToken(User user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("AspNet.Identity.SecurityStamp", user.SecurityStamp ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtOptions.DurationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public Task<bool> RoleExistsAsync(string roleName)
    {
        return _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordDto passDto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Error.NotFound("Auth.UserNotFound", "User not found.");

        var result = await _signInManager.CheckPasswordSignInAsync(user, passDto.CurrentPassword, false);
        if (!result.Succeeded)
            return Error.Validation("Auth.CurrentPasswordWrong", "Current password is wrong.");

        if (passDto.NewPassword != passDto.ConfirmPassword)
            return Error.Validation("Auth.PasswordMismatch", "The passwords do not match.");

        var ok = await _userManager.ChangePasswordAsync(user, passDto.CurrentPassword, passDto.NewPassword);
        if (!ok.Succeeded)
            return Error.Validation("Auth.PasswordChangeFailed", string.Join(", ", ok.Errors.Select(e => e.Description)));

        user.NewUser = false;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Error.Failure("Auth.UserUpdateFailed", string.Join(", ", updateResult.Errors.Select(e => e.Description)));

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(string userId, string password)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Error.NotFound("Auth.UserNotFound", "User not found.");

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
            return Error.Failure("Auth.PasswordRemoveFailed", $"Failed to remove old password: {string.Join(", ", removeResult.Errors.Select(e => e.Description))}");

        var addResult = await _userManager.AddPasswordAsync(user, password);
        if (!addResult.Succeeded)
            return Error.Validation("Auth.PasswordAddFailed", string.Join(", ", addResult.Errors.Select(e => e.Description)));

        user.NewUser = true;
        await _userManager.UpdateAsync(user);
        return Result.Success();
    }

    public async Task<Result<string>> ReGenerateTokenAsync(string userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Error.NotFound("Auth.UserNotFound", "User does not exist.");

        var roles = await _userManager.GetRolesAsync(user);
        return GenerateJwtToken(user, roles);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey))
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.ValidateToken(token, tokenValidationParameters, out _);
    }

    public async Task<Result<AuthResult>> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        var principal = GetPrincipalFromExpiredToken(dto.ExpiredToken);
        if (principal == null)
            return Error.Unauthorized("Auth.InvalidExpiredToken", "Invalid expired token");

        string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Error.NotFound("Auth.UserNotFound", "User not found");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Error.NotFound("Auth.UserNotFound", "User not found");

        if (!user.IsActive)
            return Error.Unauthorized("Auth.UserDeactivated", "User is deactivated");

        if (string.IsNullOrEmpty(user.RefreshToken) || user.RefreshToken != dto.RefreshToken)
            return Error.Unauthorized("Auth.InvalidRefreshToken", "Invalid refresh token");

        if (!user.RefreshTokenExpiryTime.HasValue || user.RefreshTokenExpiryTime < DateTime.UtcNow)
            return Error.Unauthorized("Auth.RefreshTokenExpired", "Refresh token expired");

        var roles = await _userManager.GetRolesAsync(user);
        string newJwt = GenerateJwtToken(user, roles);
        string newRefresh = GenerateRefreshToken();

        user.RefreshToken = newRefresh;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
        await _userManager.UpdateAsync(user);

        var authResponse = new AuthResponseDto
        {
            Role = roles.FirstOrDefault(),
            NewUser = user.NewUser,
            UserName = user.UserName ?? string.Empty,
            FullName = user.FullName
        };

        return new AuthResult(newJwt, newRefresh, authResponse);
    }
}
