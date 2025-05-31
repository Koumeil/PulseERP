// ChangePassword.cs
namespace PulseERP.Abstractions.Security.DTOs;

public sealed record ChangePassword(string CurrentPassword, string NewPassword);
