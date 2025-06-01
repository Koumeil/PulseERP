using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly CoreDbContext _dbContext;

    public UnitOfWork(CoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
