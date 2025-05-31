using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Users;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories.Queries;

public class UserQueryRepository : IUserQueryRepository
{
    private readonly CoreDbContext _context;

    public UserQueryRepository(CoreDbContext context)
    {
        _context = context;
    }

    public async Task<PaginationResult<User>> GetAllAsync(UserParams userParams)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(userParams.Search))
        {
            var lower = userParams.Search.ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lower)
                || u.LastName.ToLower().Contains(lower)
                || u.Email.Value.ToLower().Contains(lower)
                || u.Phone.Value.ToLower().Contains(lower)
            );
        }

        if (!string.IsNullOrWhiteSpace(userParams.Role))
            query = query.Where(u => u.Role.RoleName == userParams.Role);

        if (userParams.IsActive.HasValue)
            query = query.Where(u => u.IsActive == userParams.IsActive);

        query = userParams.Sort switch
        {
            "firstNameAsc" => query.OrderBy(u => u.FirstName),
            "firstNameDesc" => query.OrderByDescending(u => u.FirstName),
            "lastNameAsc" => query.OrderBy(u => u.LastName),
            "lastNameDesc" => query.OrderByDescending(u => u.LastName),
            "emailAsc" => query.OrderBy(u => u.Email.Value),
            "emailDesc" => query.OrderByDescending(u => u.Email.Value),
            "roleAsc" => query.OrderBy(u => u.Role.RoleName),
            "roleDesc" => query.OrderByDescending(u => u.Role.RoleName),
            _ => query.OrderBy(u => u.LastName),
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((userParams.PageNumber - 1) * userParams.PageSize)
            .Take(userParams.PageSize)
            .ToListAsync();

        return new PaginationResult<User>(items, total, userParams.PageNumber, userParams.PageSize);
    }

    public Task<User?> GetByIdAsync(Guid id) =>
        _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);

    public Task<User?> GetByEmailAsync(Email email) =>
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
