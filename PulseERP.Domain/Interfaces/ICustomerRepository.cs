using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Domain.Interfaces;

public interface ICustomerRepository
{
    Task<PagedResult<Customer>> GetAllAsync(
        PaginationParams pagination,
        CustomerFilter customerParams
    );

    Task<Customer?> GetByIdAsync(Guid id);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<int> SaveChangesAsync();
}
