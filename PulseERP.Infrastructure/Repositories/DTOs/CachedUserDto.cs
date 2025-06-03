namespace PulseERP.Infrastructure.Repositories.DTOs.User;

/// <summary>
/// DTO for caching User aggregate data in distributed cache (Redis, etc.).
/// Contains only primitive types and is JSON-serializable.
/// </summary>
public class CachedUserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!; // Always string, never VO
    public string Phone { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Role { get; set; } = default!;
    public bool IsActive { get; set; }
    public bool RequirePasswordChange { get; set; }
    public DateTime? PasswordLastChangedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int FailedLoginAttempts { get; set; }
    public DateTime? LockoutEnd { get; set; }
    // Ajoute d'autres champs si utile (claims, address, etc.)
}
