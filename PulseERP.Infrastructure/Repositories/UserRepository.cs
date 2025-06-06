using Microsoft.EntityFrameworkCore;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;
public class UserRepository : IUserRepository
{
    private readonly CoreDbContext _context;

    public UserRepository(CoreDbContext context)
    {
        _context = context;
    }

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

    public async Task<User?> FindByIdAsync(Guid id, bool bypassCache)
    {
        var user = bypassCache
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == id)
            : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user;
    }

    public async Task<User?> FindByEmailAsync(EmailAddress email, bool bypassCache)
    {
        var user = bypassCache
            ? await _context.Users.FirstOrDefaultAsync(u => u.Email == email)
            : await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);
        return user;
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user)
    {
        _context.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(Guid id) => _context.Users.AnyAsync(u => u.Id == id);
}
