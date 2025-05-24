using AutoMapper;
using PulseERP.Application.DTOs.Customers;
using PulseERP.Domain.Entities;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Customers;

public class CustomerProfile : Profile
{
    public CustomerProfile()
    {
        // Domain → DTO (lecture)
        CreateMap<Customer, CustomerDto>()
            .ForMember(
                dest => dest.Street,
                opt => opt.MapFrom(src => src.Address != null ? src.Address.Street : null)
            )
            .ForMember(
                dest => dest.City,
                opt => opt.MapFrom(src => src.Address != null ? src.Address.City : null)
            )
            .ForMember(
                dest => dest.ZipCode,
                opt => opt.MapFrom(src => src.Address != null ? src.Address.ZipCode : null)
            )
            .ForMember(
                dest => dest.Country,
                opt => opt.MapFrom(src => src.Address != null ? src.Address.Country : null)
            );

        // Command → Domain (création)
        CreateMap<CreateCustomerCommand, Customer>()
            .ConstructUsing(cmd =>
                Customer.Create(
                    cmd.FirstName,
                    cmd.LastName,
                    cmd.Email,
                    Address.TryCreateIfValid(cmd.Street, cmd.City, cmd.ZipCode, cmd.Country),
                    cmd.Phone
                )
            );
    }
}
