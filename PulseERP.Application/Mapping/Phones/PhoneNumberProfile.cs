using AutoMapper;
using PulseERP.Shared.Dtos.Phones;
using PulseERP.Domain.ValueObjects;

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
