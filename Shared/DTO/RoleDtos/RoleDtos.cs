using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.RoleDtos;

public class CreateRoleDto
{
    [Required]
    [MaxLength(256)]
    public string RoleName { get; set; } = string.Empty;
}

public class EditRoleDto
{
    [Required]
    public string RoleId { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string RoleName { get; set; } = string.Empty;
}

public class RoleDetailsDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public int UserCount { get; set; }
}

public class RoleDto
{
    public string RoleId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
