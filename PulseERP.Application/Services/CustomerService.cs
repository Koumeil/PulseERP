using AutoMapper;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Contracts.Dtos.Services;
using PulseERP.Contracts.Services;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Repositories;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _repository;
    private readonly IAppLogger<CustomerService> _logger;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository repository,
        IAppLogger<CustomerService> logger,
        IMapper mapper
    )
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<Guid>> CreateAsync(CreateCustomerCommand command)
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
            return Result<Guid>.Success(customer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError("Create failed", ex);
            return Result<Guid>.Failure(ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> GetByIdAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        return customer == null
            ? Result<CustomerDto>.Failure("Not found")
            : Result<CustomerDto>.Success(_mapper.Map<CustomerDto>(customer));
    }

    public async Task<Result<IReadOnlyList<CustomerDto>>> GetAllAsync()
    {
        var customers = await _repository.GetAllAsync();
        return Result<IReadOnlyList<CustomerDto>>.Success(
            customers.Select(_mapper.Map<CustomerDto>).ToList()
        );
    }

    public async Task<Result> UpdateAsync(Guid id, UpdateCustomerCommand command)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return Result.Failure("Customer not found");

        customer.UpdateDetails(command.FirstName, command.LastName, command.Email, command.Phone);

        customer.UpdateAddress(command.Street, command.City, command.ZipCode, command.Country);

        await _repository.UpdateAsync(customer);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(Guid id)
    {
        var customer = await _repository.GetByIdAsync(id);
        if (customer is null)
            return Result.Failure("Not found");

        await _repository.DeleteAsync(customer);
        return Result.Success();
    }
}
