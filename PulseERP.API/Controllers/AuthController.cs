using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos; 
using PulseERP.Application.Interfaces.Services;
using PulseERP.Shared.Dtos.Auth;
using PulseERP.Shared.Dtos.Auth.Token;

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

    // [HttpPost("register")]
    // public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(RegisterRequest request)
    // {
    //     var authResult = await _authService.RegisterAsync(request);
    //     var response = new ApiResponse<AuthResponse>(Success: true, Data: authResult, Error: null);
    //     return Ok(response);
    // }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        var authResult = await _authService.LoginAsync(request);
        var response = new ApiResponse<AuthResponse>(Success: true, Data: authResult, Error: null);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenRequest request
    )
    {
        var authResult = await _authService.RefreshTokenAsync(request.RefreshToken);
        var response = new ApiResponse<AuthResponse>(Success: true, Data: authResult, Error: null);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        var response = new ApiResponse<object>(Success: true, Data: null, Error: null);
        return Ok(response);
    }
}
