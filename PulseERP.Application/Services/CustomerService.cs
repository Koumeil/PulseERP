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
using PulseERP.Domain.ValueObjects.Adresses;

namespace PulseERP.Application.Services;

/// <summary>
/// Service providing all operations for Customer entities.
/// </summary>
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

    /// <summary>
    /// Retrieves a paged list of customers, optionally filtered.
    /// </summary>
    /// <param name="paginationParams">Paging parameters.</param>
    /// <param name="customerFilter">Customer filtering parameters.</param>
    /// <returns>Paged list of customer summaries.</returns>
    public async Task<PagedResult<CustomerSummary>> GetAllAsync(
        PaginationParams paginationParams,
        CustomerFilter customerFilter
    )
    {
        try
        {
            var paged = await _repo.GetAllAsync(paginationParams, customerFilter);

            var summaries = _mapper.Map<List<CustomerSummary>>(paged.Items);

            _log.LogInformation(
                "Retrieved {Count} customers (page {Page}/{PageSize}) with filter: {@Filter}",
                summaries.Count,
                paged.PageNumber,
                paged.PageSize,
                customerFilter
            );

            return new PagedResult<CustomerSummary>
            {
                Items = summaries,
                TotalItems = paged.TotalItems,
                PageNumber = paged.PageNumber,
                PageSize = paged.PageSize,
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error retrieving customer list.");
            throw new DomainException(
                "An unexpected error occurred while retrieving customers.",
                ex
            );
        }
    }

    /// <summary>
    /// Retrieves a customer's details by its unique identifier.
    /// </summary>
    /// <param name="id">Customer unique identifier.</param>
    /// <returns>Customer details.</returns>
    public async Task<CustomerDetails> GetByIdAsync(Guid id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
        {
            _log.LogWarning("Customer {CustomerId} not found.", id);
            throw new NotFoundException("Customer", id);
        }

        return _mapper.Map<CustomerDetails>(customer);
    }

    #endregion

    #region CREATE

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    /// <param name="cmd">Customer creation command.</param>
    /// <returns>Details of the newly created customer.</returns>
    public async Task<CustomerDetails> CreateAsync(CreateCustomerCommand cmd)
    {
        try
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

            _log.LogInformation(
                "Customer {CustomerId} created: {FirstName} {LastName} ({Email})",
                customer.Id,
                customer.FirstName,
                customer.LastName,
                customer.Email
            );

            return _mapper.Map<CustomerDetails>(customer);
        }
        catch (ValidationException ex)
        {
            _log.LogWarning(ex, "Validation failed during customer creation. {@Errors}", ex.Errors);
            throw;
        }
        catch (DomainException ex)
        {
            _log.LogWarning(ex, "Domain error during customer creation: {Message}", ex.Message);
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error creating customer.");
            throw new DomainException(
                "An unexpected error occurred while creating a customer.",
                ex
            );
        }
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Updates the details of an existing customer.
    /// </summary>
    /// <param name="id">Customer unique identifier.</param>
    /// <param name="cmd">Update command containing new details.</param>
    /// <returns>Updated customer details.</returns>
    public async Task<CustomerDetails> UpdateAsync(Guid id, UpdateCustomerCommand cmd)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
        {
            _log.LogWarning("Customer {CustomerId} not found for update.", id);
            throw new NotFoundException("Customer", id);
        }

        try
        {
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

            _log.LogInformation("Customer {CustomerId} updated.", customer.Id);

            return _mapper.Map<CustomerDetails>(customer);
        }
        catch (ValidationException ex)
        {
            _log.LogWarning(
                ex,
                "Validation failed during customer update for {CustomerId}. {@Errors}",
                id,
                ex.Errors
            );
            throw;
        }
        catch (DomainException ex)
        {
            _log.LogWarning(
                ex,
                "Domain error during customer update for {CustomerId}: {Message}",
                id,
                ex.Message
            );
            throw;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Unexpected error updating customer {CustomerId}.", id);
            throw new DomainException(
                "An unexpected error occurred while updating the customer.",
                ex
            );
        }
    }

    #endregion

    #region DELETE / OTHER

    /// <summary>
    /// Soft-deletes (deactivates) a customer by its ID.
    /// </summary>
    /// <param name="id">Customer unique identifier.</param>
    public async Task DeleteAsync(Guid id)
    {
        var customer = await _repo.GetByIdAsync(id);
        if (customer is null)
        {
            _log.LogWarning("Customer {CustomerId} not found for deletion.", id);
            throw new NotFoundException("Customer", id);
        }

        customer.Deactivate();
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} deactivated.", customer.Id);
    }

    /// <summary>
    /// Assigns a customer to a user.
    /// </summary>
    /// <param name="customerId">Customer unique identifier.</param>
    /// <param name="userId">User unique identifier.</param>
    public async Task AssignToUserAsync(Guid customerId, Guid userId)
    {
        var customer = await _repo.GetByIdAsync(customerId);
        if (customer is null)
        {
            _log.LogWarning("Customer {CustomerId} not found for assignment.", customerId);
            throw new NotFoundException("Customer", customerId);
        }

        customer.AssignTo(userId);
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Customer {CustomerId} assigned to user {UserId}.", customerId, userId);
    }

    /// <summary>
    /// Records an interaction for a customer (and adds a note).
    /// </summary>
    /// <param name="customerId">Customer unique identifier.</param>
    /// <param name="note">Interaction note.</param>
    public async Task RecordInteractionAsync(Guid customerId, string note)
    {
        var customer = await _repo.GetByIdAsync(customerId);
        if (customer is null)
        {
            _log.LogWarning(
                "Customer {CustomerId} not found for recording interaction.",
                customerId
            );
            throw new NotFoundException("Customer", customerId);
        }

        customer.RecordInteraction();
        customer.AddNote(note);
        await _repo.UpdateAsync(customer);
        await _repo.SaveChangesAsync();

        _log.LogInformation("Interaction recorded for customer {CustomerId}.", customerId);
    }

    #endregion
}
