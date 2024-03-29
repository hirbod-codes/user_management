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
        CreateMap<UserPermissions, UserPrivilegesRetrieveDto>().ConvertUsing<UserPrivilegesRetrieveConverter>();

        CreateMap<Reader, ReaderRetrieveDto>();
        CreateMap<Updater, UpdaterRetrieveDto>();
        CreateMap<Deleter, DeleterRetrieveDto>();
    }

    private void MapUserPrivilegesPatchDto()
    {
        CreateMap<UserPrivilegesPatchDto, UserPermissions>().ConvertUsing<UserPrivilegesPatchConverter>();

        CreateMap<ReaderPatchDto, Reader>();
        CreateMap<ReaderPatchDto[], Reader[]>();
        CreateMap<UpdaterPatchDto, Updater>();
        CreateMap<UpdaterPatchDto[], Updater[]>();
        CreateMap<DeleterPatchDto, Deleter>();
        CreateMap<DeleterPatchDto[], Deleter[]>();
    }
}

public class UserPrivilegesRetrieveConverter : ITypeConverter<UserPermissions, UserPrivilegesRetrieveDto>
{
    public UserPrivilegesRetrieveDto Convert(UserPermissions s, UserPrivilegesRetrieveDto d, ResolutionContext context) => new UserPrivilegesRetrieveDto()
    {
        Readers = s.Readers.ToList().ConvertAll<ReaderRetrieveDto>(o => new ReaderRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllReaders = s.AllReaders,
        Updaters = s.Updaters.ToList().ConvertAll<UpdaterRetrieveDto>(o => new UpdaterRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllUpdaters = s.AllUpdaters,
        Deleters = s.Deleters.ToList().ConvertAll<DeleterRetrieveDto>(o => new DeleterRetrieveDto() { Author = o.Author, AuthorId = o.AuthorId.ToString(), IsPermitted = o.IsPermitted }).ToArray(),
    };
}

public class UserPrivilegesPatchConverter : ITypeConverter<UserPrivilegesPatchDto, UserPermissions>
{
    public UserPermissions Convert(UserPrivilegesPatchDto s, UserPermissions d, ResolutionContext context) => new UserPermissions()
    {
        Readers = s.Readers == null ? new Reader[] { } : s.Readers.ToList().ConvertAll<Reader>(o => new Reader() { Author = o.Author, AuthorId = ObjectId.Parse(o.AuthorId), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllReaders = s.AllReaders,
        Updaters = s.Updaters == null ? new Updater[] { } : s.Updaters.ToList().ConvertAll<Updater>(o => new Updater() { Author = o.Author, AuthorId = ObjectId.Parse(o.AuthorId), IsPermitted = o.IsPermitted, Fields = o.Fields }).ToArray(),
        AllUpdaters = s.AllUpdaters,
        Deleters = s.Deleters == null ? new Deleter[] { } : s.Deleters.ToList().ConvertAll<Deleter>(o => new Deleter() { Author = o.Author, AuthorId = ObjectId.Parse(o.AuthorId), IsPermitted = o.IsPermitted }).ToArray(),
    };
}
