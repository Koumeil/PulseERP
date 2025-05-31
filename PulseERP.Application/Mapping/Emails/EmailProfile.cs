using System;
using AutoMapper;
using PulseERP.Application.Common.Models;
using PulseERP.Domain.ValueObjects;

namespace PulseERP.Application.Mapping.Emails;

public class EmailProfile : Profile
{
    public EmailProfile()
    {
        CreateMap<EmailAddress, EmailAddressModel>()
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

        CreateMap<EmailAddressModel, EmailAddress>().ConstructUsing(dto => EmailAddress.Create(dto.Value));
    }
}
