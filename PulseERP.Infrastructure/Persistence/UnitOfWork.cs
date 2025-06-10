using MediatR;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Persistence;

public class UnitOfWork(CoreDbContext context, IMediator mediator) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<int> SaveChangesAndDispatchEventsAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAndDispatchEventsAsync(mediator, cancellationToken);
    }
}