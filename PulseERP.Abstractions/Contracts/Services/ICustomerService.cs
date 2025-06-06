using PulseERP.Abstractions.Common.DTOs.Customers.Commands;
using PulseERP.Abstractions.Common.DTOs.Customers.Models;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Interfaces;

public interface ICustomerService
{
    Task<PagedResult<CustomerSummary>> GetAllCustomersAsync(CustomerFilter customerFilter);
    Task<CustomerDetails> FindCustomerByIdAsync(Guid id);
    Task<CustomerDetails> FindCustomerByEmailAsync(EmailAddress email);
    Task<CustomerDetails> CreateCustomerAsync(CreateCustomerCommand cmd);
    Task<CustomerDetails> UpdateCustomerAsync(Guid id, UpdateCustomerCommand cmd);
    Task AssignCustomerToUserAsync(Guid customerId, Guid userId);
    Task RecordCustomerInteractionAsync(Guid customerId, string note);
    Task ActivateCustomerAsync(Guid id);
    Task DeactivateCustomerAsync(Guid id);
    Task RestoreCustomerAsync(Guid id);
    Task DeleteCustomerAsync(Guid id);
}
