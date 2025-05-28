using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Domain.Pagination;
using PulseERP.Shared.Dtos.Users;

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

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginationResult<UserDto>>>> GetAll(
        [FromQuery] PaginationParams paginationParams
    )
    {
        var result = await _userService.GetAllAsync(paginationParams);

        var response = new ApiResponse<PaginationResult<UserDto>>(
            Success: true,
            Data: result,
            Message: "Users retrieved successfully"
        );

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetById(Guid id)
    {
        var result = await _userService.GetByIdAsync(id);

        var response = new ApiResponse<UserDto>(
            Success: true,
            Data: result,
            Message: "User retrieved successfully"
        );

        return Ok(response);
    }

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

        return CreatedAtAction(nameof(GetById), new { id = response.Data!.Id }, response);
    }

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

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(Guid id)
    {
        await _userService.DeleteAsync(id);

        var reponse = new ApiResponse<object>(
            Success: true,
            Data: null,
            Message: "User deleted successfully"
        );
        return Ok(reponse);
    }
}
