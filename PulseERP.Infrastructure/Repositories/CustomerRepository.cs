using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CoreDbContext _ctx;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(CoreDbContext ctx, ILogger<CustomerRepository> logger)
    {
        _ctx = ctx;
        _logger = logger;
    }

    public async Task<PagedResult<Customer>> GetAllAsync(
        PaginationParams pagination,
        CustomerFilter customerFilter
    )
    {
        var query = _ctx.Customers.AsNoTracking();

        // Recherche texte (sur tous les champs utiles)
        if (!string.IsNullOrWhiteSpace(customerFilter.Search))
        {
            var lower = customerFilter.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(lower)
                || c.LastName.ToLower().Contains(lower)
                || c.CompanyName.ToLower().Contains(lower)
                || c.Email.Value.ToLower().Contains(lower)
            );
        }
        // Filtrage par status
        if (
            !string.IsNullOrWhiteSpace(customerFilter.Status)
            && Enum.TryParse<CustomerStatus>(customerFilter.Status, out var status)
        )
            query = query.Where(c => c.Status == status);

        // Filtrage par type
        if (
            !string.IsNullOrWhiteSpace(customerFilter.Type)
            && Enum.TryParse<CustomerType>(customerFilter.Type, out var type)
        )
            query = query.Where(c => c.Type == type);

        // Filtrage par VIP
        if (customerFilter.IsVIP.HasValue)
            query = query.Where(c => c.IsVIP == customerFilter.IsVIP);

        // Filtrage par user assigné
        if (customerFilter.AssignedToUserId.HasValue)
            query = query.Where(c => c.AssignedToUserId == customerFilter.AssignedToUserId);

        var total = await query.CountAsync();
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

    public async Task<Customer?> GetByIdAsync(Guid id) =>
        await _ctx.Customers.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);

    public Task AddAsync(Customer customer)
    {
        _ctx.Customers.Add(customer);
        _logger.LogInformation("Customer {CustomerId} added to context", customer.Id);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Customer customer)
    {
        _ctx.Customers.Update(customer);
        _logger.LogInformation("Customer {CustomerId} updated in context", customer.Id);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Customer customer)
    {
        _ctx.Customers.Remove(customer);
        _logger.LogInformation("Customer {CustomerId} removed from context", customer.Id);
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
