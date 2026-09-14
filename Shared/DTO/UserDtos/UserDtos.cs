namespace Shared.DTO.UserDtos;

public class UserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? UserCode { get; set; }
    public bool IsActive { get; set; }
    public List<RoleDto>? Roles { get; set; }
}

public class NewUserDto
{
    public string UserCode { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string Password { get; set; } = string.Empty;
    public string RoleId { get; set; } = string.Empty;
}

public class EditUserDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserCode { get; set; } = string.Empty;
    public string? UserName { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string RoleId { get; set; } = string.Empty;
}

public class RoleDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
