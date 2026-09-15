using DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Interfaces;

public interface IRoleRepository
{
    Task<List<IdentityRole>> GetAllRolesAsync();
    Task<IdentityRole?> GetRoleByIdAsync(string roleId);
    Task<IdentityRole?> GetRoleByNameAsync(string roleName);
    Task<IdentityResult> CreateRoleAsync(string roleName);
    Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName);
    Task<IdentityResult> DeleteRoleAsync(string roleId);
    Task<bool> RoleExistsAsync(string roleId);
    Task<List<User>> GetUsersInRoleAsync(string roleName);
    Task<int> GetUserCountInRoleAsync(string roleName);
    Task<IList<string>> GetUserRolesAsync(string userId);
    Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName);
    Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName);
    Task<IdentityResult> SetUserRolesAsync(string userId, IEnumerable<string> roleNames);
    Task<IDbContextTransaction> GenerateTransaction();
}
