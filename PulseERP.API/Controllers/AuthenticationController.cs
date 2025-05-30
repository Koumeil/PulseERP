using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PulseERP.API.Dtos;
using PulseERP.Domain.Dtos.Auth;
using PulseERP.Domain.Dtos.Auth.Password;
using PulseERP.Domain.Dtos.Auth.Token;
using PulseERP.Domain.Interfaces.Services;

namespace PulseERP.API.Controllers
{
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
        public async Task<ActionResult<ApiResponse<object>>> Logout(
            [FromBody] LogoutRequest request
        )
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new ApiResponse<object>(true, null, null));
        }

        // ----------------------------------------------------------------
        // PASSWORD RESET FLOW
        // ----------------------------------------------------------------

        /// <summary>
        /// Envoie un e-mail avec un token pour réinitialiser le mot de passe
        /// </summary>
        [HttpPost("request-password-reset")]
        public async Task<ActionResult<ApiResponse<object>>> RequestPasswordReset(
            [FromBody] RequestPasswordResetDto request
        )
        {
            await _passwordService.RequestPasswordResetAsync(request.Email);
            // On ne révèle pas si l'email existe ou non
            return Ok(
                new ApiResponse<object>(
                    true,
                    null,
                    "If the email exists, a reset link has been sent."
                )
            );
        }

        /// <summary>
        /// Réinitialise le mot de passe à l'aide du token reçu par e-mail
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPasswordWithToken(
            [FromBody] ResetPasswordWithTokenDto request
        )
        {
            await _passwordService.ResetPasswordWithTokenAsync(request.Token, request.NewPassword);
            return Ok(new ApiResponse<object>(true, null, "Password has been reset successfully."));
        }

        // ----------------------------------------------------------------
        // CHANGE PASSWORD (utilisateur connecté)
        // ----------------------------------------------------------------

        /// <summary>
        /// Change le mot de passe de l'utilisateur authentifié
        /// </summary>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<ActionResult<ApiResponse<object>>> ChangePassword(
            [FromBody] ChangePasswordDto request
        )
        {
            // On est certain·e·s d'être authentifié·e ici
            var sub =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new UnauthorizedAccessException("Missing NameIdentifier claim.");

            if (!Guid.TryParse(sub, out var userId))
                throw new UnauthorizedAccessException("Invalid 'sub' claim.");

            // Appel métier
            await _passwordService.ChangePasswordAsync(
                userId,
                request.CurrentPassword,
                request.NewPassword
            );

            return Ok(
                new ApiResponse<object>(
                    Success: true,
                    Data: null,
                    Message: "Password changed successfully.",
                    Error: null
                )
            );
        }
    }
}
