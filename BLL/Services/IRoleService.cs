using BLL.CommonResult;
using Shared.DTO.RoleDtos;
using Shared.DTO.UserRoleDtos;

namespace BLL.Services;

public interface IRoleService
{
    Task<Result<List<RoleDetailsDto>>> GetAllRolesAsync();
    Task<Result<RoleDetailsDto>> GetRoleByIdAsync(string roleId);
    Task<Result> CreateRoleAsync(CreateRoleDto dto);
    Task<Result> UpdateRoleAsync(EditRoleDto dto);
    Task<Result> DeleteRoleAsync(string roleId);
    Task<Result<List<UserRoleInfoDto>>> GetUsersInRoleAsync(string roleName);
    Task<Result<List<string>>> GetUserRolesAsync(string userId);
    Task<Result> AssignRoleToUserAsync(AssignUserRoleDto dto);
    Task<Result> RemoveRoleFromUserAsync(AssignUserRoleDto dto);
    Task<Result> SetUserRolesAsync(UpdateUserRolesDto dto);
}
