using AutoMapper;
using PulseERP.Abstractions.Common.DTOs.Users.Models;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User → UserDto

        CreateMap<User, UserSummary>()
            // Id
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("FirstName", opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam("LastName", opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email.ToString()))
            .ForCtorParam("Phone", opt => opt.MapFrom(src => src.PhoneNumber.ToString()))
            .ForCtorParam("Role", opt => opt.MapFrom(src => src.Role.ToString()))
            .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive))
            .ForCtorParam(
                "RequirePasswordChange",
                opt => opt.MapFrom(src => src.RequirePasswordChange)
            )
            .ForCtorParam("LastLoginDate", opt => opt.MapFrom(src => src.LastLoginDate))
            .ForCtorParam("FailedLoginAttempts", opt => opt.MapFrom(src => src.FailedLoginAttempts))
            .ForCtorParam("LockoutEnd", opt => opt.MapFrom(src => src.LockoutEnd));

        // User → UserDetailsDto
        CreateMap<User, UserDetails>()
            .ForCtorParam("Id", opt => opt.MapFrom(src => src.Id))
            .ForCtorParam("FirstName", opt => opt.MapFrom(src => src.FirstName))
            .ForCtorParam("LastName", opt => opt.MapFrom(src => src.LastName))
            .ForCtorParam("Email", opt => opt.MapFrom(src => src.Email.ToString()))
            .ForCtorParam("Phone", opt => opt.MapFrom(src => src.PhoneNumber.ToString()))
            .ForCtorParam("Role", opt => opt.MapFrom(src => src.Role.ToString()))
            .ForCtorParam("IsActive", opt => opt.MapFrom(src => src.IsActive))
            .ForCtorParam("IsDeleted", opt => opt.MapFrom(src => src.IsDeleted))
            .ForCtorParam("CreatedAt", opt => opt.MapFrom(src => src.CreatedAt))
            .ForCtorParam("LastLogin", opt => opt.MapFrom(src => src.LastLoginDate))
            .ForCtorParam(
                "FailedLoginAttempts",
                opt => opt.MapFrom(src => src.FailedLoginAttempts)
            );
    }
}
