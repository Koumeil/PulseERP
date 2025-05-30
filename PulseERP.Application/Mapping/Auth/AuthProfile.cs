using AutoMapper;
using PulseERP.Domain.Dtos.Auth;
using PulseERP.Domain.Dtos.Users;

namespace PulseERP.Application.Mapping.Auth;

public class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<AuthResponse, UserDto>()
            .ConstructUsing(src => new UserDto(
                src.User.Id,
                src.User.FirstName,
                src.User.LastName,
                src.User.Email,
                src.User.Phone,
                src.User.Role
            ));

        // Mappage AuthResponse → UserInfo
        CreateMap<AuthResponse, UserInfo>()
            .ConstructUsing(src => new UserInfo(
                src.User.Id,
                src.User.FirstName,
                src.User.LastName,
                src.User.Email,
                src.User.Phone,
                src.User.Role
            ));

        // Mappage inverse si nécessaire (UserInfo → AuthResponse.User)
        CreateMap<UserInfo, AuthResponse>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.AccessToken, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());
    }
}
