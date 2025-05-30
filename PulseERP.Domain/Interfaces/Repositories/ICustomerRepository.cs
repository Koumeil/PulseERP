using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Domain.Interfaces.Repositories;

public interface ICustomerRepository
{
    Task<PaginationResult<Customer>> GetAllAsync(PaginationParams pagination);
    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<int> SaveChangesAsync();
}
