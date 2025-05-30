using AutoMapper;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.Dtos.Address;

namespace PulseERP.Application.Mapping.Addresses;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressDto, Address>()
            .ConstructUsing(dto => new Address(dto.Street, dto.City, dto.ZipCode, dto.Country));
    }
}
