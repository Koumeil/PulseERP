using Microsoft.EntityFrameworkCore;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CoreDbContext _ctx;

    public CustomerRepository(CoreDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<PaginationResult<Customer>> GetAllAsync(PaginationParams pagination)
    {
        var query = _ctx.Customers.AsNoTracking();
        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.LastName)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();
        return new PaginationResult<Customer>(
            items,
            total,
            pagination.PageNumber,
            pagination.PageSize
        );
    }

    public async Task<Customer?> GetByIdAsync(Guid id) =>
        await _ctx.Customers.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);

    public Task AddAsync(Customer customer)
    {
        _ctx.Customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {
        _ctx.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer customer)
    {
        _ctx.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
