using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class UserCommandRepository : IUserCommandRepository
{
    private readonly CoreDbContext _context;

    public UserCommandRepository(CoreDbContext context)
    {
        _context = context;
    }

    public Task AddAsync(User user)
    {
        return _context.Users.AddAsync(user).AsTask();
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
}
