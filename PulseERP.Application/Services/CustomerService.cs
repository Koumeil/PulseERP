using AutoMapper;
using PulseERP.Application.Common;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Application.Interfaces;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IMapper _mapper;

    public CustomerService(ICustomerRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
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

        // Construire les ValueObjects si présents
        Email? newEmail = null;
        Phone? newPhone = null;
        Address? newAddress = null;
        CustomerType? newType = null;
        CustomerStatus? newStatus = null;

        if (!string.IsNullOrWhiteSpace(request.Email))
            newEmail = Email.Create(request.Email);

        if (!string.IsNullOrWhiteSpace(request.Phone))
            newPhone = Phone.Create(request.Phone);

        if (!string.IsNullOrWhiteSpace(request.Address))
            newAddress = Address.Create(request.Address);

        if (!string.IsNullOrWhiteSpace(request.Type))
            newType = Enum.Parse<CustomerType>(request.Type);

        if (!string.IsNullOrWhiteSpace(request.Status))
            newStatus = Enum.Parse<CustomerStatus>(request.Status);

        // Mise à jour atomique de tous les champs
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

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<ServiceResult<bool>> DeleteAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult<bool>.Fail("Customer.NotFound", "Client introuvable");

        await _repository.DeleteAsync(customer);
        await _repository.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult<CustomerDto>.Fail("Customer.NotFound", "Client introuvable");

        return ServiceResult<CustomerDto>.Ok(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<PaginationResult<CustomerDto>> GetAllAsync(PaginationParams pagination)
    {
        var paged = await _repository.GetAllAsync(pagination);
        var dtos = _mapper.Map<List<CustomerDto>>(paged.Items);
        return new PaginationResult<CustomerDto>(
            dtos,
            paged.TotalItems,
            paged.PageSize,
            paged.PageNumber
        );
    }

    public async Task<ServiceResult> AssignToUserAsync(Guid customerId, Guid userId)
    {
        var customer = await _repository.GetByIdAsync(customerId);
        if (customer is null)
            return ServiceResult.Fail("Customer.NotFound", "Client introuvable");

        customer.AssignTo(userId);
        await _repository.UpdateAsync(customer);
        await _repository.SaveChangesAsync();

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

        return ServiceResult.Ok();
    }
}
