// Commands/ResetPasswordWithTokenCommand.cs
namespace PulseERP.Abstractions.Common.DTOs.Passwords.Commands;

public sealed record ResetPasswordWithTokenCommand(string Token, string NewPassword);
