using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Customers.Commands;
using PulseERP.Application.Customers.Models;

namespace PulseERP.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerSummary>> GetAllAsync(
        PaginationParams paginationParams,
        CustomerFilter customerFilter
    );
    Task<CustomerDetails> GetByIdAsync(Guid id);
    Task<CustomerDetails> CreateAsync(CreateCustomerCommand cmd);
    Task<CustomerDetails> UpdateAsync(Guid id, UpdateCustomerCommand cmd);
    Task DeleteAsync(Guid id);
    Task AssignToUserAsync(Guid customerId, Guid userId);
    Task RecordInteractionAsync(Guid customerId, string note);
}
