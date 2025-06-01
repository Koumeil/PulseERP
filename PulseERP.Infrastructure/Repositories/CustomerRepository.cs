using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="Customer"/> entities, with Redis caching on <c>GetByIdAsync</c>.
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly CoreDbContext _ctx;
    private readonly ILogger<CustomerRepository> _logger;
    private readonly IDistributedCache _cache;
    private const string CustomerByIdKeyTemplate = "CustomerRepository:Id:{0}";

    /// <summary>
    /// Initializes a new instance of <see cref="CustomerRepository"/>.
    /// </summary>
    /// <param name="ctx">EF Core DB context.</param>
    /// <param name="logger">Logger instance.</param>
    /// <param name="cache">Redis distributed cache.</param>
    public CustomerRepository(
        CoreDbContext ctx,
        ILogger<CustomerRepository> logger,
        IDistributedCache cache
    )
    {
        _ctx = ctx;
        _logger = logger;
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<PagedResult<Customer>> GetAllAsync(
        PaginationParams pagination,
        CustomerFilter customerFilter
    )
    {
        var query = _ctx.Customers.AsNoTracking();

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

    /// <inheritdoc/>
    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return null;
        }

        string cacheKey = string.Format(CustomerByIdKeyTemplate, id);
        string? cachedJson = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            return JsonSerializer.Deserialize<Customer>(cachedJson);
        }

        Customer? customer = await _ctx
            .Customers.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id);

        if (customer is not null)
        {
            string json = JsonSerializer.Serialize(customer);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30),
            };
            await _cache.SetStringAsync(cacheKey, json, options);
        }

        return customer;
    }

    /// <inheritdoc/>
    public Task AddAsync(Customer customer)
    {
        _ctx.Customers.Add(customer);
        _logger.LogInformation("Customer {CustomerId} added to context", customer.Id);
        string cacheKey = string.Format(CustomerByIdKeyTemplate, customer.Id);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task UpdateAsync(Customer customer)
    {
        _ctx.Customers.Update(customer);
        _logger.LogInformation("Customer {CustomerId} updated in context", customer.Id);
        string cacheKey = string.Format(CustomerByIdKeyTemplate, customer.Id);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task DeleteAsync(Customer customer)
    {
        _ctx.Customers.Remove(customer);
        _logger.LogInformation("Customer {CustomerId} removed from context", customer.Id);
        string cacheKey = string.Format(CustomerByIdKeyTemplate, customer.Id);
        _cache.Remove(cacheKey);
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync() => _ctx.SaveChangesAsync();
}
