using AutoMapper;
using PulseERP.Contracts.Dtos.Users;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Mapping User → UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.ToString()))
            .ForMember(
                dest => dest.Phone,
                opt => opt.MapFrom(src => src.Phone != null ? src.Phone.ToString() : null)
            );

        // Mapping CreateUserRequest → User via factory User.Create
        CreateMap<CreateUserRequest, User>()
            .ConstructUsing(cmd => User.Create(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone));

        // Mapping PaginationResult<User> → PaginationResult<UserDto>
        CreateMap<PaginationResult<User>, PaginationResult<UserDto>>()
            .ConvertUsing(
                (src, dest, context) =>
                {
                    var mappedItems = context.Mapper.Map<List<UserDto>>(src.Items);
                    return new PaginationResult<UserDto>(
                        mappedItems,
                        src.TotalItems,
                        src.PageNumber,
                        src.PageSize
                    );
                }
            );
    }
}
