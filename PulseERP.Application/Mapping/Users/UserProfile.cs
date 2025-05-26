using AutoMapper;
using PulseERP.Contracts.Dtos.Users;
using PulseERP.Domain.Entities;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<CreateUserRequest, User>();
    }
}
