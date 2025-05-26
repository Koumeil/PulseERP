using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Contracts.Dtos.Services;

namespace PulseERP.Contracts.Interfaces.Services;

public interface ICustomerService
{
    Task<ServiceResult<Guid>> CreateAsync(CreateCustomerRequest command);
    Task<ServiceResult<CustomerDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<IReadOnlyList<CustomerDto>>> GetAllAsync();
    Task<ServiceResult> UpdateAsync(Guid id, UpdateCustomerRequest command);
    Task<ServiceResult> DeleteAsync(Guid id);
}
