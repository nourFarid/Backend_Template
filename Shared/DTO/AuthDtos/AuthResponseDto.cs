namespace Shared.DTO.AuthDtos;

public class RefreshTokenRequestDto
{
    public string ExpiredToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

public class AuthResponseDto
{
    public string? Role { get; set; }
    public bool NewUser { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? UserCode { get; set; }
    public string? UserId { get; set; }
}
