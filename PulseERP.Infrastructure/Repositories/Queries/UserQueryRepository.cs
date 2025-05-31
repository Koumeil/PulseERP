using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
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

    public async Task<PagedResult<User>> GetAllAsync(UserFilter userFilter)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(userFilter.Search))
        {
            var lower = userFilter.Search.ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lower)
                || u.LastName.ToLower().Contains(lower)
                || u.Email.Value.ToLower().Contains(lower)
                || u.Phone.Value.ToLower().Contains(lower)
            );
        }

        if (!string.IsNullOrWhiteSpace(userFilter.Role))
            query = query.Where(u => u.Role.Value == userFilter.Role);

        if (userFilter.IsActive.HasValue)
            query = query.Where(u => u.IsActive == userFilter.IsActive);

        query = userFilter.Sort switch
        {
            "firstNameAsc" => query.OrderBy(u => u.FirstName),
            "firstNameDesc" => query.OrderByDescending(u => u.FirstName),
            "lastNameAsc" => query.OrderBy(u => u.LastName),
            "lastNameDesc" => query.OrderByDescending(u => u.LastName),
            "emailAsc" => query.OrderBy(u => u.Email.Value),
            "emailDesc" => query.OrderByDescending(u => u.Email.Value),
            "roleAsc" => query.OrderBy(u => u.Role.Value),
            "roleDesc" => query.OrderByDescending(u => u.Role.Value),
            _ => query.OrderBy(u => u.LastName),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((userFilter.PageNumber - 1) * userFilter.PageSize)
            .Take(userFilter.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            PageNumber = userFilter.PageNumber,
            PageSize = userFilter.PageSize,
            TotalItems = total,
        };
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(EmailAddress email) =>
        _context.Users.SingleOrDefaultAsync(u => u.Email == email);

    public Task<bool> ExistsAsync(Guid id) => _context.Users.AnyAsync(u => u.Id == id);

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
            .SingleOrDefaultAsync();
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
