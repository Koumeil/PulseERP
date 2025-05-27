using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Interfaces.Services;

public interface ICustomerService
{
    Task<PaginationResult<CustomerDto>> GetAllAsync(PaginationParams paginationParams);
    Task<CustomerDto> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest command);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest command);
    Task DeactivateAsync(Guid id);
}
