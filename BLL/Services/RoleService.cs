using BLL.CommonResult;
using DAL.Interfaces;
using Shared.DTO.RoleDtos;
using Shared.DTO.UserRoleDtos;

namespace BLL.Services;

public class RoleService : IRoleService
{
    private readonly IUnitOfWork _unitOfWork;
   
    public RoleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<RoleDetailsDto>>> GetAllRolesAsync()
    {
        var roles = await _unitOfWork.Roles.GetAllRolesAsync();
        var resultList = new List<RoleDetailsDto>();

        foreach (var role in roles)
        {
            var userCount = await _unitOfWork.Roles.GetUserCountInRoleAsync(role.Name!);
            resultList.Add(new RoleDetailsDto
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                UserCount = userCount,
            });
        }

        return Result<List<RoleDetailsDto>>.Success(resultList);
    }

    public async Task<Result<RoleDetailsDto>> GetRoleByIdAsync(string roleId)
    {
        var role = await _unitOfWork.Roles.GetRoleByIdAsync(roleId);
        if (role == null)
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

        var userCount = await _unitOfWork.Roles.GetUserCountInRoleAsync(role.Name!);
        return Result<RoleDetailsDto>.Success(new RoleDetailsDto
        {
            RoleId = role.Id,
            RoleName = role.Name ?? string.Empty,
            UserCount = userCount,
        });
    }

    public async Task<Result> CreateRoleAsync(CreateRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return Error.Validation("Role.NameRequired", "Role name is required.");

        if (await _unitOfWork.Roles.RoleExistsAsync(dto.RoleName.Trim()))
            return Error.Conflict("Role.AlreadyExists", "A role with this name already exists.");

        var identityResult = await _unitOfWork.Roles.CreateRoleAsync(dto.RoleName.Trim());
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.CreateFailed", errors);
        }

        return Result.Success();
    }

    public async Task<Result> UpdateRoleAsync(EditRoleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.RoleName))
            return Error.Validation("Role.NameRequired", "Role name is required.");

        var existingRole = await _unitOfWork.Roles.GetRoleByIdAsync(dto.RoleId);
        if (existingRole == null)
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

       
        var roleWithSameName = await _unitOfWork.Roles.GetRoleByNameAsync(dto.RoleName.Trim());
        if (roleWithSameName != null && roleWithSameName.Id != dto.RoleId)
            return Error.Conflict("Role.AlreadyExists", "A role with this name already exists.");

        var identityResult = await _unitOfWork.Roles.UpdateRoleAsync(dto.RoleId, dto.RoleName.Trim());
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.UpdateFailed", errors);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteRoleAsync(string roleId)
    {
        var role = await _unitOfWork.Roles.GetRoleByIdAsync(roleId);
        if (role == null)
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

            var userCount = await _unitOfWork.Roles.GetUserCountInRoleAsync(role.Name!);
        if (userCount > 0)
            return Error.Validation("Role.HasAssignedUsers", "Cannot delete role because it is assigned to active users.");

        var identityResult = await _unitOfWork.Roles.DeleteRoleAsync(roleId);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.DeleteFailed", errors);
        }

        return Result.Success();
    }

    public async Task<Result<List<UserRoleInfoDto>>> GetUsersInRoleAsync(string roleName)
    {
        if (!await _unitOfWork.Roles.RoleExistsAsync(roleName))
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

        var users = await _unitOfWork.Roles.GetUsersInRoleAsync(roleName);
        var dtoList = new List<UserRoleInfoDto>();

        foreach (var user in users)
        {
            var roles = await _unitOfWork.Roles.GetUserRolesAsync(user.Id);
            dtoList.Add(new UserRoleInfoDto
            {
                UserId = user.Id,
                UserName = user.UserName ?? string.Empty,
                FullName = user.FullName,
                Roles = roles.ToList()
            });
        }

        return Result<List<UserRoleInfoDto>>.Success(dtoList);
    }

    public async Task<Result<List<string>>> GetUserRolesAsync(string userId)
    {
        var roles = await _unitOfWork.Roles.GetUserRolesAsync(userId);
        return Result<List<string>>.Success(roles.ToList());
    }

    public async Task<Result> AssignRoleToUserAsync(AssignUserRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetRoleByIdAsync(dto.RoleId);
        if (role == null || string.IsNullOrEmpty(role.Name))
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

        var identityResult = await _unitOfWork.Roles.AssignRoleToUserAsync(dto.UserId, role.Name);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.AssignFailed", errors);
        }

        return Result.Success();
    }

    public async Task<Result> RemoveRoleFromUserAsync(AssignUserRoleDto dto)
    {
        var role = await _unitOfWork.Roles.GetRoleByIdAsync(dto.RoleId);
        if (role == null || string.IsNullOrEmpty(role.Name))
            return Error.NotFound("Role.NotFound", "The specified role was not found.");

        var identityResult = await _unitOfWork.Roles.RemoveRoleFromUserAsync(dto.UserId, role.Name);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.RemoveFailed", errors);
        }

        return Result.Success();
    }

    public async Task<Result> SetUserRolesAsync(UpdateUserRolesDto dto)
    {
        var roleNames = new List<string>();
        foreach (var roleId in dto.RoleIds)
        {
            var role = await _unitOfWork.Roles.GetRoleByIdAsync(roleId);
            if (role == null || string.IsNullOrEmpty(role.Name))
                return Error.Validation("Role.NotFound", $"Role with ID '{roleId}' does not exist.");

            roleNames.Add(role.Name);
        }

        var identityResult = await _unitOfWork.Roles.SetUserRolesAsync(dto.UserId, roleNames);
        if (!identityResult.Succeeded)
        {
            var errors = string.Join(" | ", identityResult.Errors.Select(e => e.Description));
            return Error.Failure("Role.UpdateRolesFailed", errors);
        }

        return Result.Success();
    }
}
