using AutoMapper;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.Dtos.Phones;

namespace PulseERP.Application.Mapping.Phones;

public class PhoneNumberProfile : Profile
{
    public PhoneNumberProfile()
    {
        CreateMap<Phone, PhoneNumberDto>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<PhoneNumberDto, Phone>().ConstructUsing(dto => Phone.Create(dto.Value));
    }
}
