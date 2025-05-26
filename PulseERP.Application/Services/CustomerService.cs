using AutoMapper;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Interfaces.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Interfaces.Repositories;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IAppLoggerService<CustomerService> _logger;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository repository,
        IAppLoggerService<CustomerService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<ServiceResult<Guid>> CreateAsync(CreateCustomerRequest command)
    {
        try
        {
            var customer = Customer.Create(
                command.FirstName,
                command.LastName,
                command.Email,
                new Address(
                    command.Address.Street,
                    command.Address.City,
                    command.Address.ZipCode,
                    command.Address.Country
                ),
                command.Phone
            );
            await _repository.AddAsync(customer);
            return ServiceResult<Guid>.Success(customer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create failed", ex);
            return ServiceResult<Guid>.Failure(ex.Message);
        }
    }

    public async Task<ServiceResult<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer == null
            ? ServiceResult<CustomerDto>.Failure("Not found")
            : ServiceResult<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<ServiceResult<IReadOnlyList<CustomerDto>>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();
        return ServiceResult<IReadOnlyList<CustomerDto>>.Success(
            customers.Select(_mapper.Map<CustomerDto>).ToList()
        );
    }

    public async Task<ServiceResult> UpdateAsync(Guid id, UpdateCustomerRequest command)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult.Failure("Customer not found");

        customer.UpdateDetails(command.FirstName, command.LastName, command.Email, command.Phone);
        customer.UpdateAddress(command.Street, command.City, command.ZipCode, command.Country);

        await _repository.UpdateAsync(customer);
        return ServiceResult.Success();
    }

    public async Task<ServiceResult> DeleteAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return ServiceResult.Failure("Not found");

        await _repository.DeleteAsync(customer);
        return ServiceResult.Success();
    }
}
