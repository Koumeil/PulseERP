using AutoMapper;
using PulseERP.Abstractions.Security.DTOs;
using PulseERP.Application.Users.Models;


namespace PulseERP.Application.Mapping.Auth
{
    public class AuthProfile : Profile
    {
        public AuthProfile()
        {
            // AuthResponse → UserDto (projection pour frontend si tu veux l'user directement)
            CreateMap<AuthResponse, UserSummary>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.User.Id))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.User.LastName))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.User.Phone))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.User.Role))
                // Les champs suivants sont à null car AuthResponse.UserInfo n'a pas ces infos
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.RequirePasswordChange, o => o.Ignore())
                .ForMember(d => d.LastLoginDate, o => o.Ignore())
                .ForMember(d => d.FailedLoginAttempts, o => o.Ignore())
                .ForMember(d => d.LockoutEnd, o => o.Ignore());

            // AuthResponse → UserInfo (simple mapping)
            CreateMap<AuthResponse, UserInfo>().ConstructUsing(src => src.User);

            // UserInfo → UserDto (pour injecter UserInfo là où l'appelant veut un UserDto)
            CreateMap<UserInfo, UserSummary>()
                .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
                .ForMember(d => d.FirstName, o => o.MapFrom(s => s.FirstName))
                .ForMember(d => d.LastName, o => o.MapFrom(s => s.LastName))
                .ForMember(d => d.Email, o => o.MapFrom(s => s.Email))
                .ForMember(d => d.Phone, o => o.MapFrom(s => s.Phone))
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role))
                // Les champs suivants sont à null/inconnus dans UserInfo
                .ForMember(d => d.IsActive, o => o.Ignore())
                .ForMember(d => d.RequirePasswordChange, o => o.Ignore())
                .ForMember(d => d.LastLoginDate, o => o.Ignore())
                .ForMember(d => d.FailedLoginAttempts, o => o.Ignore())
                .ForMember(d => d.LockoutEnd, o => o.Ignore());

            // UserDto → UserInfo (reverse, au cas où)
            CreateMap<UserSummary, UserInfo>()
                .ConstructUsing(src => new UserInfo(
                    src.Id,
                    src.FirstName,
                    src.LastName,
                    src.Email,
                    src.Phone,
                    src.Role
                ));
        }
    }
}
