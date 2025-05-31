using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Customers.Commands;
using PulseERP.Application.Customers.Models;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.ValueObjects;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        CreateMap<Customer, CustomerSummary>()
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone.ToString()))
            .ForMember(d => d.Address, o => o.MapFrom(s => s.Address.ToString()))
            .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Notes, o => o.MapFrom(s => s.Notes.ToList()))
            .ForMember(d => d.Tags, o => o.MapFrom(s => s.Tags.ToList()));

        CreateMap<CreateCustomerCommand, Customer>()
            .ConstructUsing(cmd =>
                Customer.Create(
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
                )
            );

        CreateMap<UpdateCustomerCommand, Customer>().ForAllMembers(o => o.Ignore());

        CreateMap<PagedResult<Customer>, PagedResult<CustomerSummary>>()
            .ConvertUsing(
                (src, dest, ctx) =>
                    new PagedResult<CustomerSummary>
                    {
                        Items = ctx.Mapper.Map<List<CustomerSummary>>(src.Items),
                        PageNumber = src.PageNumber,
                        PageSize = src.PageSize,
                        TotalItems = src.TotalItems,
                    }
            );
    }
}
