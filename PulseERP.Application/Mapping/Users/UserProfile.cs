using AutoMapper;
using PulseERP.Abstractions.Common.Pagination;
using PulseERP.Application.Users.Models;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // User → UserDto
        CreateMap<User, UserSummary>()
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
        CreateMap<User, UserDetails>()
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email.ToString()))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone.ToString()))
            .ForMember(d => d.Role, opt => opt.MapFrom(s => s.Role.ToString()))
            .ForMember(d => d.IsActive, opt => opt.MapFrom(s => s.IsActive))
            .ForMember(d => d.CreatedAt, opt => opt.MapFrom(s => s.CreatedAt))
            .ForMember(d => d.LastLogin, opt => opt.MapFrom(s => s.LastLoginDate))
            .ForMember(d => d.FailedLoginAttempts, opt => opt.MapFrom(s => s.FailedLoginAttempts));

        // PaginationResult<User> → PaginationResult<UserDto>
        CreateMap<PagedResult<User>, PagedResult<UserSummary>>()
            .ConvertUsing(
                (src, _, context) =>
                    new PagedResult<UserSummary>
                    {
                        Items = context.Mapper.Map<List<UserSummary>>(src.Items),
                        PageNumber = src.PageNumber,
                        PageSize = src.PageSize,
                        TotalItems = src.TotalItems,
                    }
            );
        CreateMap<User, UserDetails>()
            .ForCtorParam("Id", opt => opt.MapFrom(s => s.Id))
            .ForCtorParam("FirstName", opt => opt.MapFrom(s => s.FirstName))
            .ForCtorParam("LastName", opt => opt.MapFrom(s => s.LastName))
            .ForCtorParam("Email", opt => opt.MapFrom(s => s.Email.ToString()))
            .ForCtorParam("Phone", opt => opt.MapFrom(s => s.Phone.ToString()))
            .ForCtorParam("Role", opt => opt.MapFrom(s => s.Role.ToString()))
            .ForCtorParam("IsActive", opt => opt.MapFrom(s => s.IsActive))
            .ForCtorParam("CreatedAt", opt => opt.MapFrom(s => s.CreatedAt))
            .ForCtorParam("LastLogin", opt => opt.MapFrom(s => s.LastLoginDate))
            .ForCtorParam("FailedLoginAttempts", opt => opt.MapFrom(s => s.FailedLoginAttempts));

        // // PaginationResult<User> → PaginationResult<UserDetailsDto>
        // CreateMap<PagedResult<User>, PagedResult<UserDetails>>()
        //     .ConvertUsing(
        //         (src, _, context) =>
        //             new PagedResult<UserDetails>
        //             {
        //                 Items = context.Mapper.Map<List<UserDetails>>(src.Items),
        //                 PageNumber = src.PageNumber,
        //                 PageSize = src.PageSize,
        //                 TotalItems = src.TotalItems,
        //             }
        //     );
    }
}
