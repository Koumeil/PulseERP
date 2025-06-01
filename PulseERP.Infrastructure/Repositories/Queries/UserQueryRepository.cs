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
        // 1) Construire l’IQueryable<User> en mode “no tracking”
        var query = _context.Users.AsNoTracking();

        // 2) Filtre “Search” sur FirstName, LastName, Email, Phone
        if (!string.IsNullOrWhiteSpace(userFilter.Search))
        {
            var pattern = $"%{userFilter.Search}%";

            query = query.Where(u =>
                // On applique LIKE sur les strings “brutes” de la colonne Email et Phone
                EF.Functions.Like(u.FirstName, pattern)
                || EF.Functions.Like(u.LastName, pattern)
                || EF.Functions.Like(EF.Property<string>(u, "Email"), pattern)
                || EF.Functions.Like(EF.Property<string>(u, "Phone"), pattern)
            );
        }

        // 3) Filtre sur Role
        if (!string.IsNullOrWhiteSpace(userFilter.Role))
        {
            // La colonne a été nommée “RoleName” → on récupère EF.Property<string>(u, "RoleName")
            query = query.Where(u => EF.Property<string>(u, "RoleName") == userFilter.Role);
        }

        // 4) Filtre sur IsActive
        if (userFilter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == userFilter.IsActive.Value);
        }

        // 5) Tri selon userFilter.Sort
        query = userFilter.Sort switch
        {
            "firstNameAsc" => query.OrderBy(u => u.FirstName),
            "firstNameDesc" => query.OrderByDescending(u => u.FirstName),
            "lastNameAsc" => query.OrderBy(u => u.LastName),
            "lastNameDesc" => query.OrderByDescending(u => u.LastName),
            "emailAsc" => query.OrderBy(u => EF.Property<string>(u, "Email")),
            "emailDesc" => query.OrderByDescending(u => EF.Property<string>(u, "Email")),
            "roleAsc" => query.OrderBy(u => EF.Property<string>(u, "RoleName")),
            "roleDesc" => query.OrderByDescending(u => EF.Property<string>(u, "RoleName")),
            _ => query.OrderBy(u => u.LastName),
        };

        // 6) Comptage total
        var total = await query.CountAsync();

        // 7) Pagination + materialization
        var items = await query
            .Skip((userFilter.PageNumber - 1) * userFilter.PageSize)
            .Take(userFilter.PageSize)
            .ToListAsync(); // ← EF réhydrate chacun en User, Email/Phone sont convertis via HasConversion

        // 8) Retour du PagedResult<User>
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
        User? user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
        return user;
    }

    /// <inheritdoc/>
    public Task<User?> GetByEmailAsync(EmailAddress email)
    {
        return _context.Users.SingleOrDefaultAsync(u => u.Email == email);
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
