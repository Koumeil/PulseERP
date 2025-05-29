using Microsoft.EntityFrameworkCore;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Infrastructure.Database;

namespace PulseERP.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly CoreDbContext _context;
    private readonly ISerilogAppLoggerService<CustomerRepository> _logger;

    public CustomerRepository(
        CoreDbContext context,
        ISerilogAppLoggerService<CustomerRepository> logger
    )
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginationResult<Customer>> GetAllAsync(PaginationParams paginationParams)
    {
        var query = _context.Customers.AsNoTracking();

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return new PaginationResult<Customer>(
            items,
            totalCount,
            paginationParams.PageNumber,
            paginationParams.PageSize
        );
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context
                .Customers.Include(c => c.Address)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error fetching product by ID: {id}", ex);
            throw;
        }
    }

    public async Task AddAsync(Customer customer)
    {
        try
        {
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            _context.Entry(customer).State = EntityState.Detached;

            _logger.LogInformation($"Customer added: {customer.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to add customer: {ex.Message}", ex);
            throw;
        }
    }

    public async Task UpdateAsync(Customer customer)
    {
        try
        {
            var existing = await _context.Customers.FirstOrDefaultAsync(c => c.Id == customer.Id);

            if (existing is null)
            {
                _logger.LogWarning($"Customer not found for update: {customer.Id}");
                throw new KeyNotFoundException("Customer not found");
            }

            _context.Entry(existing).CurrentValues.SetValues(customer);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Customer updated: {customer.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to update customer {customer.Id}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task DeleteAsync(Customer customer)
    {
        try
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            _logger.LogInformation($"Customer deleted: {customer.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to delete customer {customer.Id}: {ex.Message}", ex);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Customers.AsNoTracking().AnyAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyList<Customer>> SearchAsync(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Array.Empty<Customer>();

        return await _context
            .Customers.AsNoTracking()
            .Where(c =>
                c.FirstName.Contains(term)
                || c.LastName.Contains(term)
                || (c.Email != null && c.Email.ToString().Contains(term))
            )
            .ToListAsync();
    }
}
