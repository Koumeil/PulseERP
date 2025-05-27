using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CoreDbContext _context;

    public UserRepository(CoreDbContext context)
    {
        _context = context;
    }

    // Méthode GetAllAsync avec pagination
    public async Task<PaginationResult<User>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _context.DomainUsers.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PaginationResult<User>(
            items,
            totalItems,
            paginationParams.PageNumber,
            paginationParams.PageSize
        );
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.DomainUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task AddAsync(User user)
    {
        await _context.DomainUsers.AddAsync(user);
        await _context.SaveChangesAsync();
        _context.Entry(user).State = EntityState.Detached;
    }

    public async Task UpdateAsync(User user)
    {
        var existing = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == user.Id);

        if (existing is null)
            throw new KeyNotFoundException($"User {user.Id} not found");

        _context.Entry(existing).CurrentValues.SetValues(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(User user)
    {
        _context.DomainUsers.Remove(user);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.DomainUsers.AnyAsync(u => u.Id == id);
    }
}
