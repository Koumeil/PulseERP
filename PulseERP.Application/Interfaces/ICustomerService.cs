using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Interfaces;

public interface ICustomerService
{
    Task<PaginationResult<CustomerDto>> GetAllAsync(PaginationParams pagination);
    Task<ServiceResult<CustomerDto>> GetByIdAsync(Guid id);
    Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerRequest request);
    Task<ServiceResult<CustomerDto>> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task<ServiceResult<bool>> DeleteAsync(Guid id);
    Task<ServiceResult> AssignToUserAsync(Guid customerId, Guid userId);
    Task<ServiceResult> RecordInteractionAsync(Guid customerId, string note);
}
