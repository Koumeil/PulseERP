using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;

namespace PulseERP.Abstractions.Security.Interfaces;

public interface ITokenRepository
{
    Task AddAsync(TokenEntity tokenEntity);
    Task<TokenEntity?> GetByTokenAndTypeAsync(string tokenHash, TokenType tokenType);
    Task<TokenEntity?> GetActiveByUserIdAndTypeAsync(Guid userId, TokenType tokenType);
    Task RevokeAllByUserIdAndTypeAsync(Guid userId, TokenType tokenType);
}