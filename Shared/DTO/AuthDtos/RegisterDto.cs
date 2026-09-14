using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.AuthDtos;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = string.Empty;

    public string? FullName { get; set; }
    public string? UserCode { get; set; }
}
