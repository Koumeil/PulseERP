using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Common.Models;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Mapping.Addresses;

public class AddressProfile : Profile
{
    public AddressProfile()
    {
        CreateMap<AddressModel, Address>()
            .ConstructUsing(dto => new Address(dto.Street, dto.City, dto.ZipCode, dto.Country));
    }
}
