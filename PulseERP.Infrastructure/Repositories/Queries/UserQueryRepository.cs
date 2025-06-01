using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Token;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories.Queries;

/// <summary>
/// Read-only repository for <see cref="User"/> entities, with Redis caching on single-entity reads.
/// </summary>
public class UserQueryRepository : IUserQueryRepository
{
    private readonly CoreDbContext _context;
    private readonly IDistributedCache _cache;
    private const string UserByIdKeyTemplate = "UserQueryRepository:Id:{0}";
    private const string UserByEmailKeyTemplate = "UserQueryRepository:Email:{0}";

    /// <summary>
    /// Initializes a new instance of <see cref="UserQueryRepository"/>.
    /// </summary>
    /// <param name="context">EF Core DB context.</param>
    /// <param name="cache">Redis distributed cache.</param>
    public UserQueryRepository(CoreDbContext context, IDistributedCache cache)
    {
        _context = context;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<User>> GetAllAsync(UserFilter userFilter)
    {
        var query = _context.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(userFilter.Search))
        {
            string lower = userFilter.Search.ToLowerInvariant();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(lower)
                || u.LastName.ToLower().Contains(lower)
                || u.Email.Value.ToLower().Contains(lower)
                || u.Phone.Value.ToLower().Contains(lower)
            );
        }

        if (!string.IsNullOrWhiteSpace(userFilter.Role))
        {
            query = query.Where(u => u.Role.Value == userFilter.Role);
        }

        if (userFilter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == userFilter.IsActive.Value);
        }

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

        int total = await query.CountAsync();
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

    /// <inheritdoc/>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        string cacheKey = string.Format(UserByIdKeyTemplate, id);
        string? cachedJson = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<User>(cachedJson);
        }

        User? user = await _context.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == id);

        if (user is not null)
        {
            string json = JsonSerializer.Serialize(user);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            };
            await _cache.SetStringAsync(cacheKey, json, options);
        }

        return user;
    }

    /// <inheritdoc/>
    public async Task<User?> GetByEmailAsync(EmailAddress email)
    {
        if (email is null)
        {
            return null;
        }

        string normalized = email.Value.Trim().ToLowerInvariant();
        string cacheKey = string.Format(UserByEmailKeyTemplate, normalized);
        string? cachedJson = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<User>(cachedJson);
        }

        User? user = await _context
            .Users.AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email.Value.ToLower() == normalized);

        if (user is not null)
        {
            string json = JsonSerializer.Serialize(user);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            };
            await _cache.SetStringAsync(cacheKey, json, options);
        }

        return user;
    }

    /// <inheritdoc/>
    public Task<bool> ExistsAsync(Guid id) => _context.Users.AnyAsync(u => u.Id == id);

    /// <inheritdoc/>
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

    /// <inheritdoc/>
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
