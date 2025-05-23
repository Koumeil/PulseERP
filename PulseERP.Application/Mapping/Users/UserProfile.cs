using AutoMapper;
using PulseERP.Application.DTOs.Users;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserCommand, User>();
    }
}
