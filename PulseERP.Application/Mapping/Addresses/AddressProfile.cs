using AutoMapper;
using PulseERP.Application.Common.Models;
using PulseERP.Domain.ValueObjects.Adresses;

namespace PulseERP.Application.Mapping.Addresses;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressModel, Address>()
            .ConstructUsing(dto => new Address(dto.Street, dto.City, dto.ZipCode, dto.Country));
    }
}
