using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories.Queries;

public class UserQueryRepository : IUserQueryRepository
{
    private readonly CoreDbContext _context;

    public UserQueryRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<PaginationResult<User>> GetAllAsync(PaginationParams pagination)
    {
        var query = _context.Users.AsNoTracking(); // lecture pure ici = OK
        var totalCount = await query.CountAsync();

        var users = await query
            .OrderBy(u => u.LastName)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PaginationResult<User>(
            users,
            totalCount,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);
    }

    public Task<bool> ExistsAsync(Guid id)
    {
        return _context.Users.AnyAsync(u => u.Id == id);
    }

    public Task<User?> GetByEmailAsync(Email email)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.Email == email);
    }

    public Task<User?> GetByRefreshTokenAsync(string refreshTokenHash)
    {
        return _context
            .RefreshTokens.Where(rt =>
                rt.Token == refreshTokenHash
                && rt.TokenType == TokenType.Refresh
                && rt.Revoked == null
                && rt.Expires > DateTime.UtcNow
            )
            .Join(_context.Users, rt => rt.UserId, u => u.Id, (rt, u) => u)
            .SingleOrDefaultAsync(); // pas de .AsNoTracking ici non plus
    }

    public Task<User?> GetByResetTokenAsync(string token)
    {
        return _context
            .RefreshTokens.Where(rt =>
                rt.Token == token
                && rt.TokenType == TokenType.PasswordReset
                && rt.Revoked == null
                && rt.Expires > DateTime.UtcNow
            )
            .Join(_context.Users, rt => rt.UserId, u => u.Id, (rt, u) => u)
            .SingleOrDefaultAsync();
    }
}
