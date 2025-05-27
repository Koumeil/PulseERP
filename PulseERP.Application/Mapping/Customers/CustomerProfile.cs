using AutoMapper;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Customers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Mapping Customer → CustomerDto
        CreateMap<Customer, CustomerDto>()
            .ConstructUsing(src => new CustomerDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email.ToString(),
                src.Phone != null ? src.Phone.ToString() : null,
                src.Address != null ? src.Address.Street : null,
                src.Address != null ? src.Address.City : null,
                src.Address != null ? src.Address.ZipCode : null,
                src.Address != null ? src.Address.Country : null
            ));

        // Mapping CreateCustomerRequest → Customer
        CreateMap<CreateCustomerRequest, Customer>()
            .ConstructUsing(
                (cmd, context) =>
                {
                    var email = context.Mapper.Map<Email>(cmd.Email);
                    var phone =
                        cmd.Phone != null ? context.Mapper.Map<PhoneNumber>(cmd.Phone) : null;
                    var address = Address.Create(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country);
                    return Customer.Create(cmd.FirstName, cmd.LastName, email, phone, address);
                }
            );

        // Mapping PaginationResult<Customer> → PaginationResult<CustomerDto>
        CreateMap<PaginationResult<Customer>, PaginationResult<CustomerDto>>()
            .ConvertUsing(
                (src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<CustomerDto>>(src.Items);
                    return new PaginationResult<CustomerDto>(
                        mappedItems,
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    );
                }
            );
    }
}
