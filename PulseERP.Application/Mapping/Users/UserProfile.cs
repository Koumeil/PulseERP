using AutoMapper;
using PulseERP.Domain.Entities;
using PulseERP.Domain.Pagination;
using PulseERP.Domain.ValueObjects;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Application.Mapping.Users;

public class UserProfile : Profile
{
    public UserProfile()
    {
        // Mapping User → UserDto avec ConstructUsing (comme pour Customer)
        CreateMap<User, UserDto>()
            .ConstructUsing(src => new UserDto(
                src.Id,
                src.FirstName,
                src.LastName,
                src.Email.ToString(),
                src.Phone.ToString()
            ));

        // Mapping CreateUserRequest → User via factory User.Create
        CreateMap<CreateUserRequest, User>()
            .ConstructUsing(cmd =>
                User.Create(
                    cmd.FirstName,
                    cmd.LastName,
                    Email.Create(cmd.Email),
                    PhoneNumber.Create(cmd.Phone)
                )
            );

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
