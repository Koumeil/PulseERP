using AutoMapper;
using PulseERP.Application.Common.Models;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Phones;

public class PhoneNumberProfile : Profile
{
    public PhoneNumberProfile()
    {
        CreateMap<Phone, PhoneNumberModel>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<PhoneNumberModel, Phone>().ConstructUsing(dto => Phone.Create(dto.Value));
    }
}
