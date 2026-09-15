using System.ComponentModel.DataAnnotations;

namespace Shared.DTO.UserRoleDtos;

public class AssignUserRoleDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string RoleId { get; set; } = string.Empty;
}

public class UpdateUserRolesDto
{
    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public List<string> RoleIds { get; set; } = [];
}

public class UserRoleInfoDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = [];
}
