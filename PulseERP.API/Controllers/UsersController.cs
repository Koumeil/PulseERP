using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.DTOs.Users.Commands;
using PulseERP.Abstractions.Common.DTOs.Users.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.API.Contracts;
using PulseERP.Application.Interfaces;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/users
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserSummary>>>> GetAll(
        [FromQuery] UserFilter userFilter
    )
    {
        var result = await _userService.GetAllAsync(userFilter);
        return Ok(
            new ApiResponse<PagedResult<UserSummary>>(true, result, "Users retrieved successfully")
        );
    }

    // GET /api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDetails>>> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        return Ok(new ApiResponse<UserDetails>(true, result, "User retrieved successfully"));
    }

    // POST /api/users
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Create(
        [FromBody] CreateUserCommand request
    )
    {
        var result = await _userService.CreateAsync(request);
        var response = new ApiResponse<UserInfo>(true, result, "User created successfully");
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    // PUT /api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDetails>>> Update(
        Guid id,
        [FromBody] UpdateUserCommand request
    )
    {
        var result = await _userService.UpdateAsync(id, request);
        return Ok(new ApiResponse<UserDetails>(true, result, "User updated successfully"));
    }

    // DELETE /api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        return NoContent(); // REST standard
    }

    // PATCH /api/users/{id}/activate
    [HttpPatch("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
    {
        await _userService.ActivateUserAsync(id);
        return Ok(new ApiResponse<object>(true, null, "User activated successfully"));
    }

    // PATCH /api/users/{id}/restore
    [HttpPatch("{id:guid}/restore")]
    public async Task<ActionResult<ApiResponse<object>>> Restore(Guid id)
    {
        await _userService.RestoreUserAsync(id);
        return Ok(new ApiResponse<object>(true, null, "User restored successfully"));
    }

    // PATCH /api/users/{id}/deactivate
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await _userService.DeactivateUserAsync(id);
        return Ok(new ApiResponse<object>(true, null, "User deactivated successfully"));
    }
}
