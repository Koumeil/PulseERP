using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Domain.Dtos.Auth;
using PulseERP.Domain.Dtos.Auth.Password;
using PulseERP.Domain.Dtos.Auth.Token;
using PulseERP.Domain.Interfaces.Services;

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
        [FromBody] RefreshTokenRequest request
    )
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers["User-Agent"].ToString();
        var authResult = await _authService.RefreshTokenAsync(request.RefreshToken, ip, ua);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest request)
    {
        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new ApiResponse<object>(true, null, null));
    }

    [HttpPost("request-password-reset")]
    public async Task<ActionResult<ApiResponse<object>>> RequestPasswordReset(
        [FromBody] RequestPasswordResetDto request
    )
    {
        await _passwordService.RequestPasswordResetAsync(request.Email);
        return Ok(
            new ApiResponse<object>(true, null, "If the email exists, a reset link has been sent.")
        );
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPasswordWithToken(
        [FromBody] ResetPasswordWithTokenDto request
    )
    {
        await _passwordService.ResetPasswordWithTokenAsync(request.Token, request.NewPassword);
        return Ok(new ApiResponse<object>(true, null, "Password has been reset successfully."));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePasswordDto request
    )
    {
        var sub =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing NameIdentifier claim.");

        if (!Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid 'sub' claim.");

        await _passwordService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword
        );

        return Ok(new ApiResponse<object>(true, null, "Password changed successfully.", null));
    }
}
