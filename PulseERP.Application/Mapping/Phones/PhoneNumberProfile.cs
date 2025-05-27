using AutoMapper;
using PulseERP.Contracts.Dtos.Phones;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Phones;

public class PhoneNumberProfile : Profile
{
    public PhoneNumberProfile()
    {
        CreateMap<PhoneNumber, PhoneNumberDto>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<PhoneNumberDto, PhoneNumber>().ConstructUsing(dto => new PhoneNumber(dto.Value));
    }
}
