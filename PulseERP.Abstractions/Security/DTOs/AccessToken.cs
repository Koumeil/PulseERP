// AccessToken.cs
namespace PulseERP.Abstractions.Security.DTOs;

public sealed record AccessToken(string Token, DateTime Expires);
