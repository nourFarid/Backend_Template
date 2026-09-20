using Shared.Constants;
using Shared.DTO.RoleDtos;
using DAL.Context;
using DAL.Interfaces;
using DAL.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public UserRepository(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<User?> CreateAsync(User user, string password, string roleName)
    {
        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new ArgumentException(errors);
        }

        if (!string.IsNullOrWhiteSpace(roleName))
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
                throw new ArgumentException("This Role doesn't exist");

            await _userManager.AddToRoleAsync(user, roleName);
        }

        return user;
    }

    public async Task<User?> GetByIdAsync(string id) =>
        await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByUserNameAsync(string username) =>
        await _userManager.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.UserName == username);

    public async Task<string?> GetRoleAsync(User user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        return roles.FirstOrDefault();
    }

    public async Task<User?> UpdateAsync(User user, string roleName, string password = "")
    {
        var existingUser = await _userManager.FindByIdAsync(user.Id);
        if (existingUser == null)
            return null;

        existingUser.UserName = user.UserName;
        existingUser.Email = user.Email;
        existingUser.FullName = user.FullName;
        existingUser.UserCode = user.UserCode;
        existingUser.IsActive = user.IsActive;

        if (!string.IsNullOrEmpty(password))
        {
            existingUser.NewUser = true;
        }

        var result = await _userManager.UpdateAsync(existingUser);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new ArgumentException(errors);
        }

        var currentRoles = await _userManager.GetRolesAsync(existingUser);
        await _userManager.RemoveFromRolesAsync(existingUser, currentRoles);
        await _userManager.AddToRoleAsync(existingUser, roleName);

        if (!string.IsNullOrEmpty(password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(existingUser);
            var passwordResult = await _userManager.ResetPasswordAsync(existingUser, token, password);
            if (!passwordResult.Succeeded)
            {
                var errors = string.Join(" ", passwordResult.Errors.Select(e => e.Description));
                throw new ArgumentException(errors);
            }
        }

        return existingUser;
    }

    public async Task<bool> ChangeStatusAsync(string userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId);
        if (existingUser == null)
            return false;

        existingUser.IsActive = !existingUser.IsActive;
        var result = await _userManager.UpdateAsync(existingUser);
        return result.Succeeded;
    }

    public async Task<IdentityResult> AddToRoleAsync(User user, string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
            throw new ArgumentException("This Role doesn't exist");

        return await _userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> RemoveFromRoleAsync(User user, string roleName)
    {
        return await _userManager.RemoveFromRoleAsync(user, roleName);
    }

    public async Task<string?> ResolveRoleNameByIdAsync(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        return role?.Name;
    }

    public async Task<string?> ResolveRoleIdByNameAsync(string roleName)
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        return role?.Id;
    }

    public async Task<bool> IsUserNameTakenAsync(string username, string? excludeUserId = null)
    {
        return await _userManager.Users
            .AnyAsync(u => u.UserName == username && u.Id != excludeUserId);
    }

    public async Task<bool> DeleteAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<List<IdentityRole>> GetAllRoleAsync()
    {
        return await _roleManager.Roles
            .Where(r => r.NormalizedName != AppRoles.ITAdmin.ToUpper())
            .ToListAsync();
    }

    public async Task<List<string>> GetUserIdsInRoleAsync(string roleName)
    {
        return await (
            from userRole in _context.UserRoles
            join role in _context.Roles on userRole.RoleId equals role.Id
            where role.Name == roleName
            select userRole.UserId
        ).ToListAsync();
    }

    public async Task<IEnumerable<User>> GetAllAsync(ISpecifications<User> specs)
    {
        var query = _userManager.Users
            .Include(u => u.Roles)
            .AsNoTracking();

        var itAdminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.ITAdmin);
        var itAdminUserIds = itAdminUsers.Select(u => u.Id).ToList();
        query = query.Where(u => !itAdminUserIds.Contains(u.Id));

        query = SpecificationsEvaluator.BuildQuery(query, specs);

        return await query.ToListAsync();
    }

    public async Task<int> GetCountAsync(ISpecifications<User> specs)
    {
        var query = _userManager.Users.AsNoTracking();

        var itAdminUsers = await _userManager.GetUsersInRoleAsync(AppRoles.ITAdmin);
        var itAdminUserIds = itAdminUsers.Select(u => u.Id).ToList();
        query = query.Where(u => !itAdminUserIds.Contains(u.Id));

        if (specs != null && specs.Criteria != null)
        {
            query = query.Where(specs.Criteria);
        }

        return await query.CountAsync();
    }
}
