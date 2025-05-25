using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Repositories;

public interface ICustomerRepository
{
    Task<IReadOnlyList<Customer>> SearchAsync(string search);
    Task<IReadOnlyList<Customer>> GetAllAsync();
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<bool> ExistsAsync(Guid id);
}
