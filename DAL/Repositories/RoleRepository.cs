using DAL.Context;
using DAL.Interfaces;
using DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace DAL.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<User> _userManager;
    private readonly AppDbContext _context;

    public RoleRepository(RoleManager<IdentityRole> roleManager, UserManager<User> userManager, AppDbContext context)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task<List<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleManager.Roles.ToListAsync();
    }

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        return await _roleManager.FindByIdAsync(roleId);
    }

    public async Task<IdentityRole?> GetRoleByNameAsync(string roleName)
    {
        return await _roleManager.FindByNameAsync(roleName);
    }

    public async Task<IdentityResult> CreateRoleAsync(string roleName)
    {
        var role = new IdentityRole(roleName);
        return await _roleManager.CreateAsync(role);
    }

    public async Task<IdentityResult> UpdateRoleAsync(string roleId, string newRoleName)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

        role.Name = newRoleName;
        return await _roleManager.UpdateAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return IdentityResult.Failed(new IdentityError { Description = "Role not found." });

        return await _roleManager.DeleteAsync(role);
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<List<User>> GetUsersInRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.ToList();
    }

    public async Task<int> GetUserCountInRoleAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return 0;

        return await _context.UserRoles.CountAsync(ur => ur.RoleId == role.Id);
    }

    public async Task<IList<string>> GetUserRolesAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return [];

        return await _userManager.GetRolesAsync(user);
    }

    public async Task<IdentityResult> AssignRoleToUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        if (!await _roleManager.RoleExistsAsync(roleName))
            return IdentityResult.Failed(new IdentityError { Description = $"Role '{roleName}' does not exist." });

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> RemoveRoleFromUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        return await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> SetUserRolesAsync(string userId, IEnumerable<string> roleNames)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Description = "User not found." });

        var currentRoles = await _userManager.GetRolesAsync(user);
        var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (!removeResult.Succeeded)
            return removeResult;

        return await _userManager.AddToRolesAsync(user, roleNames);
    }

    public async Task<IDbContextTransaction> GenerateTransaction()
    {
        if (_context.Database.CurrentTransaction != null)
        {
            return _context.Database.CurrentTransaction;
        }

        return await _context.Database.BeginTransactionAsync();
    }
}
