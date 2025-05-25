using AutoMapper;
using PulseERP.Contracts.Dtos.Address;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Addresses;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressDto, Address>()
            .ConstructUsing(dto => new Address(dto.Street, dto.City, dto.ZipCode, dto.Country));
    }
}
