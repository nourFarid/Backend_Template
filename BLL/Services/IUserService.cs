using BLL.CommonResult;
using Shared.DTO;
using Shared.DTO.UserDtos;
using Shared.SpecificationsQueryParams;

namespace BLL.Services;

public interface IUserService
{
    Task<Result> CreateAsync(NewUserDto dto, string createdBy);
    Task<Result<PaginatedResult<UserDto>>> GetAllAsync(UserQueryParams queryParams);
    Task<Result<UserDto>> GetByIdAsync(string id);
    Task<Result> UpdateAsync(EditUserDto dto, string updatedBy);
    Task<Result> ChangeStatusAsync(string id, string updatedBy);
    Task<Result> DeleteAsync(string id, string deletedBy);
    Task<Result<List<RoleDto>>> GetAllRoleAsync();
    Task<bool> CheckSameRole(string user1Id, string user2Id);
}
