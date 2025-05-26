using Microsoft.AspNetCore.Mvc;
using PulseERP.Contracts.Dtos.Auth;
using PulseERP.Contracts.Dtos.Auth.Token;
using PulseERP.Contracts.Interfaces.Services;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (result.IsFailure)
            return BadRequest(result.Error);

        return Ok(result.Data);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok(result.Data);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenResponse request)
    {
        var result = await _authService.RefreshTokenAsync(request.Token, request.RefreshToken);

        if (result.IsFailure)
            return Unauthorized(result.Error);

        return Ok(result.Data);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return NoContent();
    }
}
