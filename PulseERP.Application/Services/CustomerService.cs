using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.Query.Customers;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(
        ICustomerRepository repository,
        IMapper mapper,
        ILogger<CustomerService> logger
    )
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PaginationResult<CustomerDto>> GetAllAsync(
        PaginationParams pagination,
        CustomerParams customerParams
    )
    {
        var paged = await _repository.GetAllAsync(pagination, customerParams);
        var dtos = _mapper.Map<List<CustomerDto>>(paged.Items);
        _logger.LogInformation(
            "Retrieved {Count} customers (Page {Page})",
            dtos.Count,
            customerParams.PageNumber
        );
        return new PaginationResult<CustomerDto>(
            dtos,
            paged.TotalItems,
            paged.PageSize,
            paged.PageNumber
        );
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult<CustomerDto>.Fail("Customer.NotFound", "Client introuvable");

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<ServiceResult<CustomerDto>> CreateAsync(CreateCustomerRequest request)
    {
        var customer = Customer.Create(
            request.FirstName,
            request.LastName,
            request.CompanyName,
            Email.Create(request.Email),
            Phone.Create(request.Phone),
            new Address(request.Street, request.City, request.ZipCode, request.Country),
            Enum.Parse<CustomerType>(request.Type),
            Enum.Parse<CustomerStatus>(request.Status),
            DateTime.UtcNow,
            request.IsVIP
        );

        customer.SetIndustry(request.Industry);
        customer.SetSource(request.Source);

        await _repository.AddAsync(customer);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} created", customer.Id);

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<ServiceResult<CustomerDto>> UpdateAsync(
        Guid id,
        UpdateCustomerRequest request
    )
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult<CustomerDto>.Fail("Customer.NotFound", "Client introuvable");

        // Update VO (construit uniquement si champs présents)
        Email? newEmail = !string.IsNullOrWhiteSpace(request.Email)
            ? Email.Create(request.Email)
            : null;
        Phone? newPhone = !string.IsNullOrWhiteSpace(request.Phone)
            ? Phone.Create(request.Phone)
            : null;
        Address? newAddress =
            (
                request.Street != null
                && request.City != null
                && request.ZipCode != null
                && request.Country != null
            )
                ? new Address(request.Street, request.City, request.ZipCode, request.Country)
                : null;
        CustomerType? newType = !string.IsNullOrWhiteSpace(request.Type)
            ? Enum.Parse<CustomerType>(request.Type)
            : null;
        CustomerStatus? newStatus = !string.IsNullOrWhiteSpace(request.Status)
            ? Enum.Parse<CustomerStatus>(request.Status)
            : null;

        customer.UpdateDetails(
            firstName: request.FirstName,
            lastName: request.LastName,
            companyName: request.CompanyName,
            email: newEmail,
            phone: newPhone,
            address: newAddress,
            type: newType,
            status: newStatus,
            isVIP: request.IsVIP,
            industry: request.Industry,
            source: request.Source
        );

        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} updated", customer.Id);

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult<bool>.Fail("Customer.NotFound", "Client introuvable");

        // Soft delete recommandé (désactivation)
        customer.Deactivate();
        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} deactivated", customer.Id);

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult> AssignToUserAsync(Guid customerId, Guid userId)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer is null)
            return ServiceResult.Fail("Customer.NotFound", "Client introuvable");

        customer.AssignTo(userId);
        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        _logger.LogInformation(
            "Customer {CustomerId} assigned to user {UserId}",
            customerId,
            userId
        );

        return ServiceResult.Ok();
    }

    public async Task<ServiceResult> RecordInteractionAsync(Guid customerId, string note)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer is null)
            return ServiceResult.Fail("Customer.NotFound", "Client introuvable");

        customer.RecordInteraction();
        customer.AddNote(note);
        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} interaction recorded", customerId);

        return ServiceResult.Ok();
    }
}
