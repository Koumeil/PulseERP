namespace PulseERP.Abstractions.Security.DTOs;

public class ActivateAccountRequest
{
    public string Token { get; set; } = default!;
    public string Password { get; set; } = default!;
    public string ConfirmPassword { get; set; } = default!;
}
