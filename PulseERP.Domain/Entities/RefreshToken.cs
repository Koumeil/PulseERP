using PulseERP.Domain.Enums;
using PulseERP.Domain.Interfaces.Services;

public class RefreshToken
{
    public Guid Id { get; private set; }
    public string Token { get; set; }
    public Guid UserId { get; set; }
    public DateTime Expires { get; set; }
    public TokenType TokenType { get; set; }
    public DateTime? Revoked { get; set; }

    // Nouvelles propriétés
    public string? CreatedByIp { get; private set; }
    public string? CreatedByUserAgent { get; private set; }

    private readonly IDateTimeProvider _dateTimeProvider;

    // Constructeur utilisé en Domain
    // Constructeur principal pour OAuth (inclut IP et UserAgent)
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
    // Appelé depuis PasswordResetTokenRepository.StoreAsync
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
