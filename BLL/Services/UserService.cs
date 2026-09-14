using BLL.CommonResult;
using BLL.Specifications.UserSpecifications;
using DAL.Interfaces;
using DAL.Model;
using Mapster;
using Shared.DTO;
using Shared.DTO.UserDtos;
using Shared.SpecificationsQueryParams;

namespace BLL.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> CreateAsync(NewUserDto dto, string createdBy)
    {
        if (!int.TryParse(dto.UserCode, out var code) || code <= 0)
            return Error.Validation("User.Validation.EmployeeCode", "Employee code must be a positive number.");

        dto.UserName = "GS" + dto.UserCode;
        if (await _unitOfWork.Users.IsUserNameTakenAsync(dto.UserName))
            return Error.Validation("User.Validation.UsernameTaken", "Username already exists.");

        var roleName = await _unitOfWork.Users.ResolveRoleNameByIdAsync(dto.RoleId);
        if (roleName == null || roleName == "IT Admin")
            return Error.Validation("User.Validation.RoleNotFound", "The specified role does not exist.");

        var entity = dto.Adapt<User>();
        entity.NewUser = true;
        entity.IsActive = true;

        var createdUser = await _unitOfWork.Users.CreateAsync(entity, dto.Password, roleName);
        if (createdUser == null)
            return Error.Failure("User.Failure.Create", "Could not create user.");

        return Result.Success();
    }

    public async Task<Result<PaginatedResult<UserDto>>> GetAllAsync(UserQueryParams queryParams)
    {
        List<string>? userIdsInRole = null;
        if (!string.IsNullOrEmpty(queryParams.Role))
        {
            userIdsInRole = await _unitOfWork.Users.GetUserIdsInRoleAsync(queryParams.Role);
        }

        var specs = new UserSpecifications(queryParams, userIdsInRole);

        var filteredUserEntities = await _unitOfWork.Users.GetAllAsync(specs);
        var totalCount = await _unitOfWork.Users.GetCountAsync(specs);

        var dtoList = filteredUserEntities.Adapt<List<UserDto>>();

        var paginatedResult = new PaginatedResult<UserDto>(
            dtoList,
            totalCount,
            queryParams.PageIndex,
            queryParams.PageSize
        );

        return Result<PaginatedResult<UserDto>>.Success(paginatedResult);
    }

    public async Task<Result<UserDto>> GetByIdAsync(string id)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null)
            return Error.NotFound("User.NotFound", "User not found.");

        var roleName = await _unitOfWork.Users.GetRoleAsync(entity);
        if (roleName != null)
        {
            var roleId = await _unitOfWork.Users.ResolveRoleIdByNameAsync(roleName);
            entity.Roles =
            [
                new Role { RoleId = roleId ?? string.Empty, RoleName = roleName }
            ];
        }

        var dto = entity.Adapt<UserDto>();
        return dto;
    }

    public async Task<Result> UpdateAsync(EditUserDto dto, string updatedBy)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId!);
        if (user == null)
            return Error.NotFound("User.NotFound", "User does not exist.");

        if (!int.TryParse(dto.UserCode, out var code) || code <= 0)
            return Error.Validation("User.Validation.EmployeeCode", "Employee code must be a positive number.");

        if (string.IsNullOrWhiteSpace(dto.Email))
            dto.Email = string.Empty;

        dto.UserName = "GS" + dto.UserCode;
        if (await _unitOfWork.Users.IsUserNameTakenAsync(dto.UserName, dto.UserId))
            return Error.Validation("User.Validation.UsernameTaken", "Username already exists.");

        var roleName = await _unitOfWork.Users.ResolveRoleNameByIdAsync(dto.RoleId);
        if (roleName == null || roleName == "IT Admin")
            return Error.Validation("User.Validation.RoleNotFound", "The specified role does not exist.");

        dto.Adapt(user);

        var updatedUser = await _unitOfWork.Users.UpdateAsync(user, roleName, dto.Password ?? string.Empty);
        if (updatedUser == null)
            return Error.Failure("User.Failure.Update", "Could not update user.");

        return Result.Success();
    }

    public async Task<Result> ChangeStatusAsync(string id, string updatedBy)
    {
        var entity = await _unitOfWork.Users.GetByIdAsync(id);
        if (entity == null)
            return Error.NotFound("User.NotFound", "User not found.");

        var currentUserRole = await _unitOfWork.Users.GetRoleAsync(entity);
        if (currentUserRole == "IT Admin")
            return Error.Validation("User.Validation.CannotChangeAdminStatus", "Cannot change status of IT Admin user.");

        var updated = await _unitOfWork.Users.ChangeStatusAsync(entity.Id);
        if (!updated)
            return Error.Failure("User.Failure", "Could not change user status.");

        return Result.Success();
    }

    public async Task<Result> DeleteAsync(string id, string deletedBy)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id);
        if (user == null)
            return Error.NotFound("User.NotFound", "User not found.");

        var userRole = await _unitOfWork.Users.GetRoleAsync(user);
        if (userRole == "IT Admin")
            return Error.Validation("User.Validation.CannotDeleteAdmin", "Cannot delete IT Admin user.");

        var result = await _unitOfWork.Users.DeleteAsync(id);
        if (!result)
            return Error.Failure("User.Failure.Delete", "Could not delete user.");

        return Result.Success();
    }

    public async Task<Result<List<RoleDto>>> GetAllRoleAsync()
    {
        var roles = await _unitOfWork.Users.GetAllRoleAsync();
        var dto = roles.Adapt<List<RoleDto>>();
        return dto;
    }

    public async Task<bool> CheckSameRole(string user1Id, string user2Id)
    {
        var user1 = await _unitOfWork.Users.GetByIdAsync(user1Id);
        var user2 = await _unitOfWork.Users.GetByIdAsync(user2Id);
        if (user1 == null || user2 == null)
            throw new ArgumentException("One or both users do not exist.");

        var user1Role = await _unitOfWork.Users.GetRoleAsync(user1);
        var user2Role = await _unitOfWork.Users.GetRoleAsync(user2);
        return user1Role == user2Role;
    }
}
