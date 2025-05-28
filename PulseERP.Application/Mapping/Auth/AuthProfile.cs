using AutoMapper;
using PulseERP.Shared.Dtos.Auth;
using PulseERP.Shared.Dtos.Users;

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
                null
            ));

        // Mappage AuthResponse → UserInfo
        CreateMap<AuthResponse, UserInfo>()
            .ConstructUsing(src => new UserInfo(
                src.User.Id,
                src.User.FirstName,
                src.User.LastName,
                src.User.Email
            ));

        // Mappage inverse si nécessaire (UserInfo → AuthResponse.User)
        CreateMap<UserInfo, AuthResponse>()
            .ForMember(dest => dest.User, opt => opt.MapFrom(src => src))
            .ForMember(dest => dest.accessTokenDto, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshTokenDto, opt => opt.Ignore());
    }
}
