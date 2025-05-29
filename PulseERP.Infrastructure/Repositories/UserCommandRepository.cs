using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class UserCommandRepository : IUserCommandRepository
{
    private readonly CoreDbContext _ctx;

    public UserCommandRepository(CoreDbContext ctx) => _ctx = ctx;

    public Task AddAsync(User user)
    {
        _ctx.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(User user)
    {
        _ctx.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(User user)
    {
        _ctx.Users.Remove(user);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
