namespace PulseERP.Abstractions.Contracts.Repositories;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAndDispatchEventsAsync(CancellationToken cancellationToken = default);
}