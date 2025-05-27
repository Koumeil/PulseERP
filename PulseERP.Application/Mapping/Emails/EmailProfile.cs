using System;
using AutoMapper;
using PulseERP.Contracts.Dtos.Emails;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Emails;

public class EmailProfile : Profile
{
    public EmailProfile()
    {
        CreateMap<Email, EmailDto>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<EmailDto, Email>().ConstructUsing(dto => new Email(dto.Value));
    }
}
