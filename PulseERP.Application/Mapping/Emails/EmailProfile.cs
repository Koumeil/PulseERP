using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Common.Models;
using PulseERP.Domain.VO;

namespace PulseERP.Application.Mapping.Emails;

public class EmailProfile : Profile
{
    public EmailProfile()
    {
        CreateMap<EmailAddress, EmailAddressModel>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<EmailAddressModel, EmailAddress>()
            .ConstructUsing(dto => new EmailAddress(dto.Value));
    }
}
