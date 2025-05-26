using AutoMapper;
using PulseERP.Contracts.Dtos.Customers;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Customers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
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

        // Command → Domain (création)
        CreateMap<CreateCustomerRequest, Customer>()
            .ConstructUsing(
                (cmd, context) =>
                {
                    var address = context.Mapper.Map<Address>(cmd.Address);
                    return Customer.Create(
                        cmd.FirstName,
                        cmd.LastName,
                        cmd.Email,
                        address,
                        cmd.Phone
                    );
                }
            );
    }
}
