using AutoMapper;
using Microsoft.Extensions.Logging;
using PulseERP.Application.Exceptions;
using PulseERP.Application.Interfaces.Services;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository repository,
        ILogger<CustomerService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PaginationResult<CustomerDto>> GetAllAsync(PaginationParams paginationParams)
    {
        var pagedCustomers = await _repository.GetAllAsync(paginationParams);
        var customerDtos = _mapper.Map<PaginationResult<CustomerDto>>(pagedCustomers);
        return customerDtos;
    }

    public async Task<CustomerDto> GetByIdAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            throw new NotFoundException($"Customer with Id '{id}' not found.", id);

        var customerDto = _mapper.Map<CustomerDto>(customer);
        return customerDto;
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest command)
    {
        var address = Address.Create(
            command.Street,
            command.City,
            command.ZipCode,
            command.Country
        );

        var email = _mapper.Map<Email>(command.Email);
        var phoneNumber = _mapper.Map<PhoneNumber>(command.Phone);

        var customer = Customer.Create(
            command.FirstName,
            command.LastName,
            email,
            phoneNumber,
            address
        );
        await _repository.AddAsync(customer);

        _logger.LogInformation("Created new customer with Id {CustomerId}", customer.Id);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest command)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            throw new NotFoundException($"Customer with Id '{id}' not found.", id);

        var email = _mapper.Map<Email>(command.Email);
        var phoneNumber = _mapper.Map<PhoneNumber>(command.Phone);
        var address = customer.Address.Update(
            command.Street,
            command.City,
            command.ZipCode,
            command.Country
        );

        customer.UpdateDetails(command.FirstName, command.LastName, email, phoneNumber);
        customer.UpdateAddress(address);

        await _repository.UpdateAsync(customer);
        _logger.LogInformation("Updated customer with Id {CustomerId}", customer.Id);

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task DeactivateAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            throw new NotFoundException($"Customer with Id '{id}' not found.", id);

        customer.Deactivate();
        await _repository.UpdateAsync(customer);

        _logger.LogInformation("Deactivated customer with Id {CustomerId}", customer.Id);
    }
}
