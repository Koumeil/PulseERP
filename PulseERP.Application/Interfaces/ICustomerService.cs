using PulseERP.Domain.Pagination;
using PulseERP.Shared.Dtos.Customers;

namespace PulseERP.Application.Interfaces;

public interface ICustomerService
{
    Task<PaginationResult<CustomerDto>> GetAllAsync(PaginationParams paginationParams);
    Task<CustomerDto> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest command);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest command);
    Task DeactivateAsync(Guid id);
}
