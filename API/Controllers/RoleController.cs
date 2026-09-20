using Shared.Constants;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.RoleDtos;
using Shared.DTO.UserRoleDtos;

namespace API.Controllers;

[Authorize(Roles = $"{AppRoles.Admin},{AppRoles.ITAdmin}")]
public class RoleController : ApiBaseController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet("ListAll")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _roleService.GetAllRolesAsync();
        return HandleResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(string id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);
        return HandleResult(result);
    }

    [HttpPost("Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
    {
        var result = await _roleService.CreateRoleAsync(dto);
        return HandleResult(result, "Role.CreateSuccess");
    }

    [HttpPut("Update")]
    public async Task<IActionResult> UpdateRole([FromBody] EditRoleDto dto)
    {
        var result = await _roleService.UpdateRoleAsync(dto);
        return HandleResult(result, "Role.UpdateSuccess");
    }

    [HttpDelete("Delete/{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var result = await _roleService.DeleteRoleAsync(id);
        return HandleResult(result, "Role.DeleteSuccess");
    }

    [HttpGet("{roleName}/Users")]
    public async Task<IActionResult> GetUsersInRole(string roleName)
    {
        var result = await _roleService.GetUsersInRoleAsync(roleName);
        return HandleResult(result);
    }

    [HttpGet("User/{userId}/Roles")]
    public async Task<IActionResult> GetUserRoles(string userId)
    {
        var result = await _roleService.GetUserRolesAsync(userId);
        return HandleResult(result);
    }

    [HttpPost("AssignToUser")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignUserRoleDto dto)
    {
        var result = await _roleService.AssignRoleToUserAsync(dto);
        return HandleResult(result, "Role.AssignSuccess");
    }

    [HttpPost("RemoveFromUser")]
    public async Task<IActionResult> RemoveRoleFromUser([FromBody] AssignUserRoleDto dto)
    {
        var result = await _roleService.RemoveRoleFromUserAsync(dto);
        return HandleResult(result, "Role.RemoveSuccess");
    }

    [HttpPut("SetUserRoles")]
    public async Task<IActionResult> SetUserRoles([FromBody] UpdateUserRolesDto dto)
    {
        var result = await _roleService.SetUserRolesAsync(dto);
        return HandleResult(result, "Role.SetRolesSuccess");
    }
}
