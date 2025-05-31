using PulseERP.Abstractions.Security.Interfaces;
using PulseERP.Domain.Enums.Token;

namespace PulseERP.Domain.Entities;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; set; } = default!;
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
    public TokenType TokenType { get; set; }
    public DateTime? Revoked { get; set; }

    public string? CreatedByIp { get; private set; }
    public string? CreatedByUserAgent { get; private set; }

    private readonly IDateTimeProvider _dateTimeProvider = default!;

    // Constructeur utilisé en Domain
    public RefreshToken(
        IDateTimeProvider dateTimeProvider,
        string createdByIp,
        string createdByUserAgent
    )
    {
        _dateTimeProvider = dateTimeProvider;
        Id = Guid.NewGuid();
        CreatedByIp = createdByIp;
        CreatedByUserAgent = createdByUserAgent;
    }

    // Constructeur de compatibilité pour PasswordReset (IP/UserAgent = null)
    public RefreshToken(IDateTimeProvider dateTimeProvider)
        : this(dateTimeProvider, createdByIp: null!, createdByUserAgent: null!)
    {
        // Pas de capture IP/UserAgent pour un PasswordResetToken
    }

    // Constructeur vide pour EF Core
    protected RefreshToken() { }

    public bool IsActive =>
        Revoked == null && Expires > (_dateTimeProvider?.UtcNow ?? DateTime.UtcNow);
}
