using PulseERP.Application.DTOs.Customers;
using PulseERP.Domain.Shared;

namespace PulseERP.Application.Interfaces;

public interface ICustomerService
{
    Task<Result<Guid>> CreateAsync(CreateCustomerCommand command);
    Task<Result<CustomerDto>> GetByIdAsync(Guid id);
    Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync();
    Task<Result> UpdateAsync(Guid id, UpdateCustomerCommand command);
    Task<Result> DeleteAsync(Guid id);
}
