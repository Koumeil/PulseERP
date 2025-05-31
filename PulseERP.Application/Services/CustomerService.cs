using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Abstractions.Common.Filters;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Customers.Commands;
using PulseERP.Application.Customers.Models;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Errors;
using PulseERP.Domain.Interfaces;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

/// <inheritdoc />
public sealed class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repo;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _log;

    public CustomerService(ICustomerRepository repo, IMapper mapper, ILogger<CustomerService> log)
    {
        _repo = repo;
        _mapper = mapper;
        _log = log;
    }

    #region READ

    public async Task<PagedResult<CustomerSummary>> GetAllAsync(
        PaginationParams paginationParams,
        CustomerFilter customerFilter
    )
    {
        var paged = await _repo.GetAllAsync(paginationParams, customerFilter);

        var summaries = _mapper.Map<IReadOnlyList<CustomerSummary>>(paged.Items);

        _log.LogInformation(
            "Retrieved {Count} customers (page {Page})",
            summaries.Count,
            paged.PageNumber
        );

        return new PagedResult<CustomerSummary>
        {
            Items = summaries,
            TotalItems = paged.TotalItems,
            PageNumber = paged.PageNumber,
            PageSize = paged.PageSize,
        };
    }

    public async Task<CustomerDetails> GetByIdAsync(Guid id)
    {
        var customer = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Customer", id);
        return _mapper.Map<CustomerDetails>(customer);
    }

    #endregion
    #region CREATE

    public async Task<CustomerDetails> CreateAsync(CreateCustomerCommand cmd)
    {
        var customer = Customer.Create(
            cmd.FirstName,
            cmd.LastName,
            cmd.CompanyName,
            EmailAddress.Create(cmd.Email),
            Phone.Create(cmd.Phone),
            new Address(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country),
            Enum.Parse<CustomerType>(cmd.Type),
            Enum.Parse<CustomerStatus>(cmd.Status),
            DateTime.UtcNow,
            cmd.IsVIP
        );

        customer.SetIndustry(cmd.Industry);
        customer.SetSource(cmd.Source);

        await _repo.AddAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} created", customer.Id);

        return _mapper.Map<CustomerDetails>(customer);
    }

    #endregion
    #region UPDATE

    public async Task<CustomerDetails> UpdateAsync(Guid id, UpdateCustomerCommand cmd)
    {
        var customer = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        customer.UpdateDetails(
            firstName: cmd.FirstName,
            lastName: cmd.LastName,
            companyName: cmd.CompanyName,
            email: cmd.Email is null ? null : EmailAddress.Create(cmd.Email),
            phone: cmd.Phone is null ? null : Phone.Create(cmd.Phone),
            address: (cmd.Street, cmd.City, cmd.ZipCode, cmd.Country) switch
            {
                ({ } s, { } c, { } z, { } co) => new Address(s, c, z, co),
                _ => null,
            },
            type: cmd.Type is null ? null : Enum.Parse<CustomerType>(cmd.Type),
            status: cmd.Status is null ? null : Enum.Parse<CustomerStatus>(cmd.Status),
            isVIP: cmd.IsVIP,
            industry: cmd.Industry,
            source: cmd.Source
        );

        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} updated", customer.Id);

        return _mapper.Map<CustomerDetails>(customer);
    }

    #endregion
    #region DELETE / OTHER

    public async Task DeleteAsync(Guid id)
    {
        var customer = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("Customer", id);

        customer.Deactivate();
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} deactivated", customer.Id);
    }

    public async Task AssignToUserAsync(Guid customerId, Guid userId)
    {
        var customer =
            await _repo.GetByIdAsync(customerId)
            ?? throw new NotFoundException("Customer", customerId);

        customer.AssignTo(userId);
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} assigned to user {UserId}", customerId, userId);
    }

    public async Task RecordInteractionAsync(Guid customerId, string note)
    {
        var customer =
            await _repo.GetByIdAsync(customerId)
            ?? throw new NotFoundException("Customer", customerId);

        customer.RecordInteraction();
        customer.AddNote(note);
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Interaction recorded for customer {CustomerId}", customerId);
    }

    #endregion
}
