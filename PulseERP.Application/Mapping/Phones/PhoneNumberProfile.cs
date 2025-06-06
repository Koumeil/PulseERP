using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Common.Models;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Mapping.Phones;

public class PhoneNumberProfile : Profile
{
    public PhoneNumberProfile()
    {
        CreateMap<Phone, PhoneNumberModel>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<PhoneNumberModel, Phone>().ConstructUsing(dto => new Phone(dto.Value));
    }
}
