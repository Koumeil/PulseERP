using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class UserQueryRepository : IUserQueryRepository
{
    private readonly CoreDbContext _ctx;

    public UserQueryRepository(CoreDbContext ctx) => _ctx = ctx;

    public async Task<PaginationResult<User>> GetAllAsync(PaginationParams pagination)
    {
        var q = _ctx.Users.AsNoTracking();
        var total = await q.CountAsync();
        var items = await q.OrderBy(u => u.LastName)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();
        return new PaginationResult<User>(items, total, pagination.PageNumber, pagination.PageSize);
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        _ctx.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);

    public Task<bool> ExistsAsync(Guid id) => _ctx.Users.AnyAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(Email email) =>
        _ctx.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByRefreshTokenAsync(string refreshTokenHash) =>
        _ctx
            .RefreshTokens.Where(rt =>
                rt.Token == refreshTokenHash
                && rt.TokenType == TokenType.Refresh
                && rt.Revoked == null
                && rt.Expires > DateTime.UtcNow
            )
            .Join(_ctx.Users, rt => rt.UserId, u => u.Id, (rt, u) => u)
            .AsNoTracking()
            .SingleOrDefaultAsync();

    public Task<User?> GetByResetTokenAsync(string token) =>
        _ctx
            .RefreshTokens.Where(rt =>
                rt.Token == token
                && rt.TokenType == TokenType.PasswordReset
                && rt.Revoked == null
                && rt.Expires > DateTime.UtcNow
            )
            .Join(_ctx.Users, rt => rt.UserId, u => u.Id, (rt, u) => u)
            .AsNoTracking()
            .SingleOrDefaultAsync();
}
