namespace user_management.Profiles;

using AutoMapper;
using user_management.Dtos.Client;
using user_management.Dtos.Token;
using user_management.Models;

class ClientProfile : Profile
{
    public ClientProfile()
    {
        MapClientCreateDto();
        MapClientRetrieveDto();
        MapClientPatchDto();
    }

    private void MapClientCreateDto()
    {
        CreateMap<ClientCreateDto, Client>();

        CreateMap<TokenPrivilegesCreateDto, TokenPrivileges>();
    }

    private void MapClientRetrieveDto()
    {
        CreateMap<Client, ClientRetrieveDto>();
        CreateMap<Client, ClientPublicInfoRetrieveDto>();
    }

    private void MapClientPatchDto()
    {
        CreateMap<ClientPatchDto, Client>();
    }
}
