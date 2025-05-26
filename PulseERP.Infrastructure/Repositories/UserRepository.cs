using Microsoft.EntityFrameworkCore;
using PulseERP.Contracts.Interfaces.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly CoreDbContext _context;
    private readonly IAppLoggerService<UserRepository> _logger;

    public UserRepository(CoreDbContext context, IAppLoggerService<UserRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.DomainUsers.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching user by ID: {id}", ex);
            throw;
        }
    }

    public async Task<IReadOnlyList<User>> GetAllAsync()
    {
        try
        {
            return await _context.DomainUsers.AsNoTracking().ToListAsync() as IReadOnlyList<User>;
        }
        catch (Exception ex)
        {
            _logger.LogError("Error fetching all users", ex);
            throw;
        }
    }

    public async Task AddAsync(User user)
    {
        try
        {
            await _context.DomainUsers.AddAsync(user);
            await _context.SaveChangesAsync();
            _context.Entry(user).State = EntityState.Detached;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error adding user {user.Id}", ex);
            throw;
        }
    }

    public async Task UpdateAsync(User user)
    {
        try
        {
            var existing = await _context.DomainUsers.FirstOrDefaultAsync(u => u.Id == user.Id);

            if (existing is null)
            {
                _logger.LogWarning($"User {user.Id} not found for update");
                throw new KeyNotFoundException($"User {user.Id} not found");
            }

            _context.Entry(existing).CurrentValues.SetValues(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User {user.Id} updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error updating user {user.Id}", ex);
            throw;
        }
    }

    public async Task DeleteAsync(User user)
    {
        try
        {
            _context.DomainUsers.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"User {user.Id} deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting user {user.Id}", ex);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            return await _context.DomainUsers.AnyAsync(u => u.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error checking existence for user {id}", ex);
            throw;
        }
    }
}
