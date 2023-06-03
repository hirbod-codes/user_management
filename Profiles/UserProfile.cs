namespace user_management.Profiles;

using AutoMapper;
using user_management.Dtos.User;
using user_management.Models;

class UserProfile : Profile
{
    public UserProfile()
    {
        MapUserCreateDto();
    }

    private void MapUserCreateDto()
    {
        CreateMap<UserCreateDto, User>();
    }

}