using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Customers;
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

    public async Task<PaginationResult<Customer>> GetAllAsync(
        PaginationParams pagination,
        CustomerParams customerParams
    )
    {
        var query = _ctx.Customers.AsNoTracking();

        // Recherche texte (sur tous les champs utiles)
        if (!string.IsNullOrWhiteSpace(customerParams.Search))
        {
            var lower = customerParams.Search.ToLowerInvariant();
            query = query.Where(c =>
                c.FirstName.ToLower().Contains(lower)
                || c.LastName.ToLower().Contains(lower)
                || c.CompanyName.ToLower().Contains(lower)
                || c.Email.Value.ToLower().Contains(lower)
            );
        }
        // Filtrage par status
        if (
            !string.IsNullOrWhiteSpace(customerParams.Status)
            && Enum.TryParse<CustomerStatus>(customerParams.Status, out var status)
        )
            query = query.Where(c => c.Status == status);

        // Filtrage par type
        if (
            !string.IsNullOrWhiteSpace(customerParams.Type)
            && Enum.TryParse<CustomerType>(customerParams.Type, out var type)
        )
            query = query.Where(c => c.Type == type);

        // Filtrage par VIP
        if (customerParams.IsVIP.HasValue)
            query = query.Where(c => c.IsVIP == customerParams.IsVIP);

        // Filtrage par user assigné
        if (customerParams.AssignedToUserId.HasValue)
            query = query.Where(c => c.AssignedToUserId == customerParams.AssignedToUserId);

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(c => c.LastName)
            .Skip((customerParams.PageNumber - 1) * customerParams.PageSize)
            .Take(customerParams.PageSize)
            .ToListAsync();

        return new PaginationResult<Customer>(
            items,
            total,
            customerParams.PageNumber,
            customerParams.PageSize
        );
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
