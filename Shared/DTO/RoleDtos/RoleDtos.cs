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
    public bool IsSystemRole { get; set; }
}

public class AssignUserRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RoleName { get; set; } = string.Empty;
}

public class UpdateUserRolesDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public List<string> RoleNames { get; set; } = [];
}

public class UserRoleInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
