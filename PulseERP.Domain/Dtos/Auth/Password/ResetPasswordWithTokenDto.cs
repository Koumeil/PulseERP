namespace PulseERP.Domain.Dtos.Auth.Password;

public class ResetPasswordWithTokenDto
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
