using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Customers.Commands;
using PulseERP.Abstractions.Common.DTOs.Customers.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Enums.Customer;
using PulseERP.Domain.VO;

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
            .ConstructUsing(cmd => new Customer(
                cmd.FirstName,
                cmd.LastName,
                cmd.CompanyName,
                new EmailAddress(cmd.Email),
                new Phone(cmd.Phone),
                new Address(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country),
                Enum.Parse<CustomerType>(cmd.Type),
                Enum.Parse<CustomerStatus>(cmd.Status),
                DateTime.UtcNow,
                cmd.IsVIP,
                null, // industry
                null, // source
                null, // lastInteractionDate
                null // assignedToUserId
            ));

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

        CreateMap<Customer, CustomerDetails>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.CompanyName))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Value))
            .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone.Value))
            .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Industry, opt => opt.MapFrom(src => src.Industry))
            .ForMember(dest => dest.Source, opt => opt.MapFrom(src => src.Source))
            .ForMember(dest => dest.IsVIP, opt => opt.MapFrom(src => src.IsVIP))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}
