using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Contracts.Dtos.Services;

namespace PulseERP.Contracts.Services;

public interface ICustomerService
{
    Task<Result<Guid>> CreateAsync(CreateCustomerCommand command);
    Task<Result<CustomerDto>> GetByIdAsync(Guid id);
    Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync();
    Task<Result> UpdateAsync(Guid id, UpdateCustomerCommand command);
    Task<Result> DeleteAsync(Guid id);
}
