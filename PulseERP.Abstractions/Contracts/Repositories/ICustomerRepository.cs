using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.VO;

namespace PulseERP.Abstractions.Contracts.Repositories;

public interface ICustomerRepository
{
    Task<PagedResult<Customer>> GetAllAsync(CustomerFilter customerParams);
    Task<Customer?> FindByIdAsync(Guid id);
    Task<Customer?> FindByEmailAsync(EmailAddress email);
    Task AddAsync(Customer customer);
    Task UpdateAsync(Customer customer);
    Task DeleteAsync(Customer customer);
    Task<int> SaveChangesAsync();
}
