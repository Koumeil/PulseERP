using Microsoft.AspNetCore.Mvc;
using PulseERP.API.DTOs.Users;
using PulseERP.Application.DTOs.Users;
using PulseERP.Application.Interfaces;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost]
    public async Task<ActionResult<UserResponse>> Create(CreateUserRequest request)
    {
        var id = await _userService.CreateAsync(
            new CreateUserCommand(request.FirstName, request.LastName, request.Email, request.Phone)
        );

        // Retourne 201 Created avec l'URL du nouvel utilisateur
        return CreatedAtAction(nameof(GetById), new { id = id }, new { Id = id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateUserRequest request)
    {
        var success = await _userService.UpdateAsync(
            new UpdateUserCommand(
                id,
                request.FirstName,
                request.LastName,
                request.Email,
                request.Phone
            )
        );

        if (!success)
            return NotFound();

        return NoContent(); // 204 No Content car la ressource est mise à jour
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _userService.DeleteAsync(id);
        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponse>> GetById(Guid id)
    {
        var user = await _userService.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return Ok(ToResponse(user));
    }

    [HttpGet]
    public async Task<ActionResult<List<UserResponse>>> GetAll()
    {
        var users = await _userService.GetAllAsync();
        return Ok(users.Select(ToResponse).ToList());
    }

    private static UserResponse ToResponse(UserDto dto) =>
        new(dto.Id, dto.FirstName, dto.LastName, dto.Email, dto.Phone, dto.IsActive);
}
