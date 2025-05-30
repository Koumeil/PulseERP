using System;
using AutoMapper;
using PulseERP.Domain.ValueObjects;
using PulseERP.Domain.Dtos.Emails;

namespace PulseERP.Application.Mapping.Emails;

public class EmailProfile : Profile
{
    public EmailProfile()
    {
        CreateMap<Email, EmailDto>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<EmailDto, Email>().ConstructUsing(dto => Email.Create(dto.Value));
    }
}
