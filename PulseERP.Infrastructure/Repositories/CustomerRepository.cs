using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Abstractions.Contracts.Repositories;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.VO;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class CustomerRepository(CoreDbContext ctx, ILogger<CustomerRepository> logger) : ICustomerRepository
{
    private readonly ILogger<CustomerRepository> _logger = logger;

    public async Task<PagedResult<Customer>> GetAllAsync(CustomerFilter customerFilter)
    {
        var query = ctx.Customers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(customerFilter.Search))
        {
            string lower = customerFilter.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(lower)
                || c.LastName.ToLower().Contains(lower)
                || c.CompanyName.ToLower().Contains(lower)
                || c.Email.Value.ToLower().Contains(lower)
            );
        }

        if (
            !string.IsNullOrWhiteSpace(customerFilter.Status)
            && Enum.TryParse<CustomerStatus>(customerFilter.Status, out var status)
        )
        {
            query = query.Where(c => c.Status == status);
        }

        if (
            !string.IsNullOrWhiteSpace(customerFilter.Type)
            && Enum.TryParse<CustomerType>(customerFilter.Type, out var type)
        )
        {
            query = query.Where(c => c.Type == type);
        }

        if (customerFilter.IsVIP.HasValue)
            query = query.Where(c => c.IsVIP == customerFilter.IsVIP.Value);

        if (customerFilter.AssignedToUserId.HasValue)
        {
            query = query.Where(c => c.AssignedToUserId == customerFilter.AssignedToUserId.Value);
        }

        int total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.LastName)
            .Skip((customerFilter.PageNumber - 1) * customerFilter.PageSize)
            .Take(customerFilter.PageSize)
            .ToListAsync();

        return new PagedResult<Customer>
        {
            Items = items,
            PageNumber = customerFilter.PageNumber,
            PageSize = customerFilter.PageSize,
            TotalItems = total,
        };
    }

    public async Task<Customer?> FindByIdAsync(Guid id)
    {
        return await ctx.Customers.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> FindByEmailAsync(EmailAddress email)
    {
        return await ctx.Customers.AsNoTracking().SingleOrDefaultAsync(c => c.Email == email);
    }

    public Task AddAsync(Customer customer)
    {
        ctx.Customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {
        ctx.Customers.Update(customer);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer customer)
    {
        ctx.Customers.Remove(customer);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => ctx.SaveChangesAsync();
}