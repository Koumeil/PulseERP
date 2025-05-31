using AutoMapper;
using PulseERP.Application.Dtos.Customer;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.ToString()))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.ToString()))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes.ToList()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags.ToList()));

        CreateMap<CreateCustomerRequest, Customer>()
            .ConstructUsing(cmd =>
                Customer.Create(
                    cmd.FirstName,
                    cmd.LastName,
                    cmd.CompanyName,
                    Email.Create(cmd.Email),
                    Phone.Create(cmd.Phone),
                    new Address(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country),
                    Enum.Parse<CustomerType>(cmd.Type),
                    Enum.Parse<CustomerStatus>(cmd.Status),
                    DateTime.UtcNow,
                    cmd.IsVIP
                )
            );

        CreateMap<UpdateCustomerRequest, Customer>().ForAllMembers(o => o.Ignore());

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
