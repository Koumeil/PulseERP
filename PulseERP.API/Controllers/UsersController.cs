using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Dtos.User;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Users;

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

    // GET api/users?pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<UserDto>>>> GetAll(
        [FromQuery] UserParams userParams
    )
    {
        var result = await _userService.GetAllAsync(userParams);
        var response = new ApiResponse<PaginationResult<UserDto>>(
            Success: true,
            Data: result,
            Message: "Users retrieved successfully"
        );
        return Ok(response);
    }

    // GET api/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDetailsDto>>> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);
        var response = new ApiResponse<UserDetailsDto>(
            Success: true,
            Data: result,
            Message: "User retrieved successfully"
        );
        return Ok(response);
    }

    // POST api/users
    [HttpPost]
    public async Task<ActionResult<ApiResponse<UserInfo>>> Create(
        [FromBody] CreateUserRequest request
    )
    {
        var result = await _userService.CreateAsync(request);
        var response = new ApiResponse<UserInfo>(
            Success: true,
            Data: result,
            Message: "User created successfully"
        );
        // renvoie 201 avec en-tête Location
        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

    // PUT api/users/{id}
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> Update(
        Guid id,
        [FromBody] UpdateUserRequest request
    )
    {
        var result = await _userService.UpdateAsync(id, request);
        var response = new ApiResponse<UserDto>(
            Success: true,
            Data: result,
            Message: "User updated successfully"
        );
        return Ok(response);
    }

    // DELETE api/users/{id}
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);
        var response = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "User deleted (deactivated) successfully"
        );
        return Ok(response);
    }

    // POST api/users/{id}/activate
    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<ApiResponse<object>>> Activate(Guid id)
    {
        await _userService.ActivateUserAsync(id);
        var response = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "User activated successfully"
        );
        return Ok(response);
    }

    // POST api/users/{id}/deactivate
    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<ApiResponse<object>>> Deactivate(Guid id)
    {
        await _userService.DeactivateUserAsync(id);
        var response = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "User deactivated successfully"
        );
        return Ok(response);
    }
}
