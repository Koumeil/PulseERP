using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.API.Contracts;
using PulseERP.Application.Passwords.Commands;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authService;
    private readonly IPasswordService _passwordService;

    public AuthenticationController(
        IAuthenticationService authService,
        IPasswordService passwordService
    )
    {
        _authService = authService;
        _passwordService = passwordService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request
    )
    {
        var authResult = await _authService.RegisterAsync(request, string.Empty, string.Empty);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(LoginRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers["User-Agent"].ToString();
        var authResult = await _authService.LoginAsync(request, ip, ua);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshToken(
        [FromBody] RefreshTokenDto request
    )
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers["User-Agent"].ToString();
        var authResult = await _authService.RefreshTokenAsync(request.Token, ip, ua);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest cmd)
    {
        await _authService.LogoutAsync(cmd.RefreshToken);
        return Ok(new ApiResponse<object>(true, null, null));
    }

    [HttpPost("request-password-reset")]
    public async Task<ActionResult<ApiResponse<object>>> RequestPasswordReset(
        [FromBody] RequestPasswordResetCommand cmd
    )
    {
        await _passwordService.RequestPasswordResetAsync(cmd.Email);
        return Ok(
            new ApiResponse<object>(true, null, "If the email exists, a reset link has been sent.")
        );
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPasswordWithToken(
        [FromBody] ResetPasswordWithTokenCommand cmd
    )
    {
        await _passwordService.ResetPasswordWithTokenAsync(cmd.Token, cmd.NewPassword);
        return Ok(new ApiResponse<object>(true, null, "Password has been reset successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePassword cmd
    )
    {
        var sub =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing NameIdentifier claim.");

        if (!Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid 'sub' claim.");

        await _passwordService.ChangePasswordAsync(userId, cmd.CurrentPassword, cmd.NewPassword);

        return Ok(new ApiResponse<object>(true, null, "Password changed successfully.", null));
    }
}
