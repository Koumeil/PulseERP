using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;
using PulseERP.Infrastructure.Persistence;

namespace PulseERP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.DomainUsers.FindAsync(id);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _context.DomainUsers.ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        await _context.DomainUsers.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.DomainUsers.Update(user);
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
