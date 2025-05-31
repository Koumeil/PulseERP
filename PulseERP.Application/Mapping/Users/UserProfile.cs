using AutoMapper;
using PulseERP.Application.Dtos.User;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User → UserDto
        CreateMap<User, UserDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone.ToString()))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(
                d => d.RequirePasswordChange,
                opt => opt.MapFrom(s => s.RequirePasswordChange)
            )
            .ForMember(d => d.LastLoginDate, opt => opt.MapFrom(s => s.LastLoginDate))
            .ForMember(d => d.FailedLoginAttempts, opt => opt.MapFrom(s => s.FailedLoginAttempts))
            .ForMember(d => d.LockoutEnd, opt => opt.MapFrom(s => s.LockoutEnd));

        // User → UserDetailsDto
        CreateMap<User, UserDetailsDto>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone.ToString()))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.LastLogin, opt => opt.MapFrom(s => s.LastLoginDate))
            .ForMember(d => d.FailedLoginAttempts, opt => opt.MapFrom(s => s.FailedLoginAttempts));

        // PaginationResult<User> → PaginationResult<UserDto>
        CreateMap<PaginationResult<User>, PaginationResult<UserDto>>()
            .ConvertUsing(
                (src, _, context) =>
                    new PaginationResult<UserDto>(
                        context.Mapper.Map<List<UserDto>>(src.Items),
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    )
            );

        // PaginationResult<User> → PaginationResult<UserDetailsDto>
        CreateMap<PaginationResult<User>, PaginationResult<UserDetailsDto>>()
            .ConvertUsing(
                (src, _, context) =>
                    new PaginationResult<UserDetailsDto>(
                        context.Mapper.Map<List<UserDetailsDto>>(src.Items),
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    )
            );
    }
}
