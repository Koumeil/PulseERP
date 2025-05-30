// Application/Mapping/Customers/CustomerProfile.cs
using AutoMapper;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Customers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Domain → DTO
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.ToString()))
            .ForMember(
                d => d.Address,
                o =>
                    o.MapFrom(s =>
                        $"{s.Address.Street}, {s.Address.City}, {s.Address.ZipCode}, {s.Address.Country}"
                    )
            )
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.LastInteractionDate, o => o.MapFrom(s => s.LastInteractionDate))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes.ToList()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags.ToList()));

        // CreateCustomerRequest → Domain
        CreateMap<CreateCustomerRequest, Customer>()
            .ConstructUsing(
                (cmd, ctx) =>
                {
                    var email = Email.Create(cmd.Email);
                    var phone = Phone.Create(cmd.Phone);
                    var address = new Address(cmd.City, cmd.Street, cmd.ZipCode, cmd.Country);
                    var type = Enum.Parse<CustomerType>(cmd.Type);
                    var status = Enum.Parse<CustomerStatus>(cmd.Status);

                    var customer = Customer.Create(
                        cmd.FirstName,
                        cmd.LastName,
                        cmd.CompanyName,
                        email,
                        phone,
                        address,
                        type,
                        status,
                        DateTime.UtcNow,
                        cmd.IsVIP
                    );
                    customer.SetIndustry(cmd.Industry);
                    customer.SetSource(cmd.Source);
                    return customer;
                }
            );

        // UpdateCustomerRequest → Domain (on ignore : on utilise UpdateDetails en service)
        CreateMap<UpdateCustomerRequest, Customer>().ForAllMembers(o => o.Ignore());

        // PaginationResult<Customer> → PaginationResult<CustomerDto>
        CreateMap<PaginationResult<Customer>, PaginationResult<CustomerDto>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PaginationResult<CustomerDto>(
                        ctx.Mapper.Map<List<CustomerDto>>(src.Items),
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    )
            );
    }
}
