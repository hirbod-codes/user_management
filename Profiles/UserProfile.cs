namespace user_management.Profiles;

using AutoMapper;
using user_management.Dtos.User;
using user_management.Models;

class UserProfile : Profile
{
    public UserProfile()
    {
        MapUserCreateDto();
        MapUserRetrieveDto();
        MapUserPrivilegesRetrieveDto();
    }

    private void MapUserCreateDto()
    {
        CreateMap<UserCreateDto, User>();
    }

    private void MapUserRetrieveDto()
    {
        CreateMap<UserClient, UserClientRetrieveDto>();
    }

    private void MapUserPrivilegesRetrieveDto()
    {
        CreateMap<UserPrivileges, UserPrivilegesRetrieveDto>().ConvertUsing<UserPrivilegesConverter>();

        CreateMap<Reader, ReaderRetrieveDto>();
        CreateMap<Updater, UpdaterRetrieveDto>();
        CreateMap<Deleter, DeleterRetrieveDto>();
    }
}

public class UserPrivilegesConverter : ITypeConverter<UserPrivileges, UserPrivilegesRetrieveDto>
{
    public UserPrivilegesRetrieveDto Convert(UserPrivileges s, UserPrivilegesRetrieveDto d, ResolutionContext context) => new UserPrivilegesRetrieveDto()
    {
        Privileges = s.Privileges,
        Readers = s.Readers.ToList().ConvertAll<ReaderRetrieveDto>(o => new ReaderRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllReaders = s.AllReaders,
        Updaters = s.Updaters.ToList().ConvertAll<UpdaterRetrieveDto>(o => new UpdaterRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllUpdaters = s.AllUpdaters,
        Deleters = s.Deleters.ToList().ConvertAll<DeleterRetrieveDto>(o => new DeleterRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted }).ToArray(),
    };
}