using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces.Persistence;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer);
    Task<Customer?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(Guid id);
    Task<IReadOnlyList<Customer>> SearchAsync(string term);
}
