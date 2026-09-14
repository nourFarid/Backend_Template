using DAL.Model;
using Microsoft.AspNetCore.Identity;

namespace DAL.Interfaces;

public interface IUserRepository
{
    Task<User?> CreateAsync(User user, string password, string roleName);
    Task<User?> GetByIdAsync(string id);
    Task<User?> GetByUserNameAsync(string username);
    Task<string?> GetRoleAsync(User user);
    Task<User?> UpdateAsync(User user, string roleName, string password = "");
    Task<bool> ChangeStatusAsync(string userId);
    Task<IdentityResult> AddToRoleAsync(User user, string roleName);
    Task<IdentityResult> RemoveFromRoleAsync(User user, string roleName);
    Task<string?> ResolveRoleNameByIdAsync(string roleId);
    Task<string?> ResolveRoleIdByNameAsync(string roleName);
    Task<List<IdentityRole>> GetAllRoleAsync();
    Task<List<string>> GetUserIdsInRoleAsync(string roleName);
    Task<IEnumerable<User>> GetAllAsync(ISpecifications<User> specs);
    Task<int> GetCountAsync(ISpecifications<User> specs);
    Task<bool> IsUserNameTakenAsync(string username, string? excludeUserId = null);
    Task<bool> DeleteAsync(string userId);
}
