using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTO.UserDtos;
using Shared.SpecificationsQueryParams;
using System.Security.Claims;

namespace API.Controllers;

public class UserController : ApiBaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> GetUserById(string userId)
    {
        var result = await _userService.GetByIdAsync(userId);
        return HandleRequest(result);
    }

    [HttpPost("Add")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> CreateUser([FromBody] NewUserDto newUser)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.CreateAsync(newUser, currentUserId);
        return HandleRequest(result, successMessageKey: "User.CreateSuccess", statusCode: StatusCodes.Status201Created);
    }

    [HttpPost("Update")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> Update([FromBody] EditUserDto user)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.UpdateAsync(user, currentUserId);
        return HandleRequest(result, successMessageKey: "User.UpdateSuccess");
    }

    [HttpPut("ChangeStatus/{id}")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> ChangeUserStatus(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.ChangeStatusAsync(id, currentUserId);
        return HandleRequest(result, successMessageKey: "User.StatusChangeSuccess");
    }

    [HttpDelete("Delete/{id}")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var result = await _userService.DeleteAsync(id, currentUserId);
        return HandleRequest(result, successMessageKey: "User.DeleteSuccess");
    }

    [HttpGet("ListAll")]
    [Authorize]
    public async Task<IActionResult> GetAllUsers([FromQuery] UserQueryParams queryParams)
    {
        queryParams ??= new UserQueryParams();
        var result = await _userService.GetAllAsync(queryParams);
        return HandleRequest(result);
    }

    [HttpGet("ListAllRoles")]
    [Authorize(Roles = "Admin,IT ADMIN")]
    public async Task<IActionResult> GetAllRoles()
    {
        var result = await _userService.GetAllRoleAsync();
        return HandleRequest(result);
    }
}
