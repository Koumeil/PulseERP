namespace PulseERP.Application.Dtos.Password;

public class ResetPasswordWithTokenDto
{
    public string Token { get; set; } = default!;
    public string NewPassword { get; set; } = default!;
}
