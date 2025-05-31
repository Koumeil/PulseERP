// Commands/ResetPasswordWithTokenCommand.cs
namespace PulseERP.Application.Passwords.Commands;

public sealed record ResetPasswordWithTokenCommand(string Token, string NewPassword);
