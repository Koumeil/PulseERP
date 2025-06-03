using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.ValueObjects.Adresses;
using PulseERP.Infrastructure.Database;
using PulseERP.Infrastructure.Repositories.DTOs.User;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Central repository for <see cref="User"/> entities.
/// Supports read and write operations with caching for single-entity queries.
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly CoreDbContext _context;
    private readonly IDistributedCache _cache;
    private readonly ILogger<UserRepository> _logger;
    private const string UserByIdKeyTemplate = "UserRepository:Id:{0}";
    private const string UserByEmailKeyTemplate = "UserRepository:Email:{0}";

    /// <summary>
    /// Initializes a new instance of <see cref="UserRepository"/>.
    /// </summary>
    public UserRepository(
        CoreDbContext context,
        IDistributedCache cache,
        ILogger<UserRepository> logger
    )
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Gets a paged, filtered list of users.
    /// </summary>
    public async Task<PagedResult<User>> GetAllAsync(UserFilter filter)
    {
        var query = _context.Users.AsNoTracking();

        // Apply search filters (FirstName, LastName, Email, Phone)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{filter.Search}%";
            query = query.Where(u =>
                EF.Functions.Like(u.FirstName, pattern)
                || EF.Functions.Like(u.LastName, pattern)
                || EF.Functions.Like(EF.Property<string>(u, "Email"), pattern)
                || EF.Functions.Like(EF.Property<string>(u, "Phone"), pattern)
            );
        }

        // Role filter
        if (!string.IsNullOrWhiteSpace(filter.Role))
        {
            query = query.Where(u => EF.Property<string>(u, "RoleName") == filter.Role);
        }

        // IsActive filter
        if (filter.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == filter.IsActive.Value);
        }

        // Sorting
        query = filter.Sort switch
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

        var total = await query.CountAsync();
        var items = await query
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize,
            TotalItems = total,
        };
    }

    /// <summary>
    /// Gets a user by Id (from cache if available).
    /// </summary>
    public async Task<User?> FindByIdAsync(Guid id, bool bypassCache)
    {
        var cacheKey = string.Format(UserByIdKeyTemplate, id);

        // 1. Lecture via cache applicatif si lecture seule
        if (!bypassCache)
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                _logger.LogDebug("User {UserId} loaded from cache.", id);
                var dto = JsonSerializer.Deserialize<CachedUserDto>(cached);
                if (dto != null)
                {
                    return User.Create(
                        dto.FirstName,
                        dto.LastName,
                        EmailAddress.Create(dto.Email),
                        Phone.Create(dto.Phone),
                        dto.PasswordHash
                    );
                }
            }
        }

        // 2. Pour update, on veut une entité propre : vide le ChangeTracker
        if (bypassCache)
            _context.ChangeTracker.Clear();

        // 3. Lecture DB : trackée (update), non-trackée (read-only)
        var user = bypassCache
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == id) // pour update
            : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id); // read-only

        // 4. Rafraîchir le cache si lecture seule et trouvé
        if (user != null && !bypassCache)
        {
            var dto = new CachedUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                Phone = user.Phone.Value,
                PasswordHash = user.PasswordHash,
                Role = user.Role.Value,
                IsActive = user.IsActive,
                RequirePasswordChange = user.RequirePasswordChange,
                PasswordLastChangedAt = user.PasswordLastChangedAt,
                LastLoginDate = user.LastLoginDate,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockoutEnd = user.LockoutEnd,
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto));
        }

        return user;
    }

    // public async Task<User?> FindByIdAsync(Guid id, bool bypassCache)
    // {
    //     var cacheKey = string.Format(UserByIdKeyTemplate, id);

    //     if (!bypassCache)
    //     {
    //         // Read from cache: retourne un objet "detaché" (User custom reconstitué) → pour affichage
    //         var cached = await _cache.GetStringAsync(cacheKey);
    //         if (cached != null)
    //         {
    //             _logger.LogDebug("User {UserId} loaded from cache.", id);
    //             var dto = JsonSerializer.Deserialize<CachedUserDto>(cached);
    //             if (dto != null)
    //             {
    //                 // ATTENTION : cet objet n’est pas tracké par EF, à utiliser uniquement pour affichage/lecture.
    //                 return User.Create(
    //                     dto.FirstName,
    //                     dto.LastName,
    //                     EmailAddress.Create(dto.Email),
    //                     Phone.Create(dto.Phone),
    //                     dto.PasswordHash
    //                 );
    //             }
    //         }
    //     }

    //     // Ici: toujours retour DB avec tracking (pour update ou si pas trouvé en cache)
    //     var user = await _context.Users.SingleOrDefaultAsync(u => u.Id == id);
    //     if (user != null && !bypassCache)
    //     {
    //         // Si on n'est PAS en bypass: refresh le cache
    //         var dto = new CachedUserDto
    //         {
    //             Id = user.Id,
    //             FirstName = user.FirstName,
    //             LastName = user.LastName,
    //             Email = user.Email.Value,
    //             Phone = user.Phone.Value,
    //             PasswordHash = user.PasswordHash,
    //             Role = user.Role.Value,
    //             IsActive = user.IsActive,
    //             RequirePasswordChange = user.RequirePasswordChange,
    //             PasswordLastChangedAt = user.PasswordLastChangedAt,
    //             LastLoginDate = user.LastLoginDate,
    //             FailedLoginAttempts = user.FailedLoginAttempts,
    //             LockoutEnd = user.LockoutEnd,
    //         };
    //         await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto));
    //     }
    //     return user;
    // }

    /// <summary>
    /// Gets a user by email (from cache if available).
    /// </summary>
    public async Task<User?> FindByEmailAsync(EmailAddress email, bool bypassCache)
    {
        var cacheKey = string.Format(UserByEmailKeyTemplate, email.Value.ToLowerInvariant());

        // 1. Si lecture simple, tente le cache applicatif
        if (!bypassCache)
        {
            var cached = await _cache.GetStringAsync(cacheKey);
            if (cached != null)
            {
                var dto = JsonSerializer.Deserialize<CachedUserDto>(cached);
                if (dto != null)
                {
                    return User.Create(
                        dto.FirstName,
                        dto.LastName,
                        EmailAddress.Create(dto.Email),
                        Phone.Create(dto.Phone),
                        dto.PasswordHash
                    );
                }
            }
        }

        // 2. Si update, on veut une entité "propre" (non polluée) : nettoyage du ChangeTracker
        if (bypassCache)
            _context.ChangeTracker.Clear();

        // 3. Lecture DB : soit trackée (update), soit non-trackée (read-only)
        var user = bypassCache
            ? await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
            : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

        // 4. On rafraîchit le cache si lecture simple et trouvée
        if (user != null && !bypassCache)
        {
            var dto = new CachedUserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email.Value,
                Phone = user.Phone.Value,
                PasswordHash = user.PasswordHash,
                Role = user.Role.Value,
                IsActive = user.IsActive,
                RequirePasswordChange = user.RequirePasswordChange,
                PasswordLastChangedAt = user.PasswordLastChangedAt,
                LastLoginDate = user.LastLoginDate,
                FailedLoginAttempts = user.FailedLoginAttempts,
                LockoutEnd = user.LockoutEnd,
            };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto));
        }

        return user;
    }

    // public async Task<User?> FindByEmailAsync(EmailAddress email, bool bypassCache = false)
    // {
    //     var cacheKey = string.Format(UserByEmailKeyTemplate, email.Value.ToLowerInvariant());

    //     if (!bypassCache)
    //     {
    //         var cached = await _cache.GetStringAsync(cacheKey);
    //         if (cached != null)
    //         {
    //             _logger.LogDebug("User {Email} loaded from cache.", email.Value);
    //             var dto = JsonSerializer.Deserialize<CachedUserDto>(cached);
    //             if (dto != null)
    //             {
    //                 return User.Create(
    //                     dto.FirstName,
    //                     dto.LastName,
    //                     EmailAddress.Create(dto.Email),
    //                     Phone.Create(dto.Phone),
    //                     dto.PasswordHash
    //                 );
    //             }
    //         }
    //     }

    //     // Toujours lire en direct de la BDD si bypassCache == true (ex: login)
    //     var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
    //     if (user != null && !bypassCache)
    //     {
    //         var dto = new CachedUserDto
    //         {
    //             Id = user.Id,
    //             FirstName = user.FirstName,
    //             LastName = user.LastName,
    //             Email = user.Email.Value,
    //             Phone = user.Phone.Value,
    //             PasswordHash = user.PasswordHash,
    //             Role = user.Role.Value,
    //             IsActive = user.IsActive,
    //             RequirePasswordChange = user.RequirePasswordChange,
    //             PasswordLastChangedAt = user.PasswordLastChangedAt,
    //             LastLoginDate = user.LastLoginDate,
    //             FailedLoginAttempts = user.FailedLoginAttempts,
    //             LockoutEnd = user.LockoutEnd,
    //         };
    //         await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dto));
    //     }
    //     return user;
    // }

    /// <summary>
    /// Checks if a user exists by Id.
    /// </summary>
    public Task<bool> ExistsAsync(Guid id) => _context.Users.AnyAsync(u => u.Id == id);

    /// <summary>
    /// Adds a new user.
    /// </summary>
    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        _logger.LogInformation("User {UserId} created at {NowLocal}.", user.Id, DateTime.Now);
    }

    /// <summary>
    /// Updates an existing user.
    /// </summary>
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        // Invalidate cache
        var cacheKey = string.Format(UserByIdKeyTemplate, user.Id);
        await _cache.RemoveAsync(cacheKey);
        var emailKey = string.Format(UserByEmailKeyTemplate, user.Email.Value.ToLowerInvariant());
        await _cache.RemoveAsync(emailKey);

        _logger.LogInformation("User {UserId} updated at {NowLocal}.", user.Id, DateTime.Now);
    }

    /// <summary>
    /// Deletes a user.
    /// </summary>
    public async Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);

        // Invalidate cache
        var cacheKey = string.Format(UserByIdKeyTemplate, user.Id);
        await _cache.RemoveAsync(cacheKey);
        var emailKey = string.Format(UserByEmailKeyTemplate, user.Email.Value.ToLowerInvariant());
        await _cache.RemoveAsync(emailKey);

        _logger.LogInformation("User {UserId} deleted at {NowLocal}.", user.Id, DateTime.Now);
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
