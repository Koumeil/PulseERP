using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces;

public interface IPasswordResetTokenRepository
{
    Task StoreAsync(Guid userId, string token, DateTime expiry);
    Task<RefreshToken?> GetActiveByTokenAsync(string token);
    Task MarkAsUsedAsync(string token);
}
