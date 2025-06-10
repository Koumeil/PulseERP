using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseERP.Abstractions.Common.DTOs.Passwords.Commands;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.API.Contracts;
using PulseERP.Domain.VO;

namespace PulseERP.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticationController(
    IAuthenticationService authService,
    IPasswordService passwordService)
    : ControllerBase
{
    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login(
        [FromBody] LoginRequest request
    )
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers["User-Agent"].ToString();
        var authResult = await authService.LoginAsync(request, ip, ua);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    // POST /api/auth/token
    [HttpPost("token")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> RefreshAccessToken(
        [FromBody] RefreshTokenDto request
    )
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
        var ua = Request.Headers["User-Agent"].ToString();
        var authResult = await authService.RefreshTokenAsync(request.Token, ip, ua);
        return Ok(new ApiResponse<AuthResponse>(true, authResult, null));
    }

    // DELETE /api/auth/token
    [HttpDelete("revoke-refresh-token")]
    public async Task<ActionResult<ApiResponse<object>>> RevokeRefreshToken(
        [FromBody] LogoutRequest cmd
    )
    {
        await authService.LogoutAsync(cmd.RefreshTokenDto);
        return Ok(new ApiResponse<object>(true, null, null));
    }

    // POST /api/auth/password-reset-request
    [HttpPost("password-reset-request")]
    public async Task<ActionResult<ApiResponse<object>>> SendPasswordResetLink(
        [FromBody] RequestPasswordResetCommand cmd
    )
    {
        await passwordService.RequestPasswordResetAsync(new EmailAddress(cmd.Email));
        return Ok(
            new ApiResponse<object>(true, null, "If the email exists, a reset link has been sent.")
        );
    }

    // POST /api/auth/password-reset
    [HttpPost("password-reset")]
    public async Task<ActionResult<ApiResponse<object>>> ResetPassword(
        [FromBody] ResetPasswordWithTokenCommand cmd
    )
    {
        await passwordService.ResetPasswordWithTokenAsync(cmd.Token, cmd.NewPassword);
        return Ok(new ApiResponse<object>(true, null, "Password has been reset successfully."));
    }

    // PATCH /api/auth/password
    [Authorize]
    [HttpPatch("password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
        [FromBody] ChangePassword cmd
    )
    {
        var sub =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("Missing NameIdentifier claim.");

        if (!Guid.TryParse(sub, out var userId))
            throw new UnauthorizedAccessException("Invalid 'sub' claim.");

        await passwordService.ChangePasswordAsync(userId, cmd.CurrentPassword, cmd.NewPassword);
        return Ok(new ApiResponse<object>(true, null, "Password changed successfully."));
    }

    // POST /api/auth/activate-account
    [HttpPost("activate-account")]
    public async Task<ActionResult<ApiResponse<object>>> ActivateAccount(
        [FromBody] ActivateAccountRequest request)
    {
        await authService.ActivateAccountAsync(request);
        return Ok(new ApiResponse<object>(true, null, "Account activated successfully."));
    }

}