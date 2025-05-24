using Microsoft.EntityFrameworkCore;
using PulseERP.Application.Common.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Persistence;
using PulseERP.Infrastructure.Persistence;

namespace PulseERP.Infrastructure.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;
    private readonly IAppLogger<CustomerRepository> _logger;

    public CustomerRepository(ApplicationDbContext context, IAppLogger<CustomerRepository> logger)
    {
        _context = context;
        _logger = logger;
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

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await _context
            .Customers.AsNoTracking() // Lecture seule
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IReadOnlyList<Customer>> GetAllAsync()
    {
        return await _context.Customers.AsNoTracking().ToListAsync();
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
