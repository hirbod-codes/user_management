namespace user_management.Profiles;

using AutoMapper;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Models;

class UserProfile : Profile
{
    public UserProfile()
    {
        MapUserCreateDto();
        MapUserRetrieveDto();
        MapUserPrivilegesRetrieveDto();
        MapUserPrivilegesPatchDto();
    }

    private void MapUserCreateDto()
    {
        CreateMap<UserCreateDto, User>();
    }

    private void MapUserRetrieveDto()
    {
        CreateMap<AuthorizedClient, AuthorizedClientRetrieveDto>();
    }

    private void MapUserPrivilegesRetrieveDto()
    {
        CreateMap<UserPermissions, UserPrivilegesRetrieveDto>();

        CreateMap<Reader, ReaderRetrieveDto>();
        CreateMap<Updater, UpdaterRetrieveDto>();
        CreateMap<Deleter, DeleterRetrieveDto>();
    }

    private void MapUserPrivilegesPatchDto()
    {
        CreateMap<UserPrivilegesPatchDto, UserPermissions>();

        CreateMap<ReaderPatchDto, Reader>();
        CreateMap<ReaderPatchDto[], Reader[]>();
        CreateMap<UpdaterPatchDto, Updater>();
        CreateMap<UpdaterPatchDto[], Updater[]>();
        CreateMap<DeleterPatchDto, Deleter>();
        CreateMap<DeleterPatchDto[], Deleter[]>();
    }
}
