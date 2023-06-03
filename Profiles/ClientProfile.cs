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
    }

    private void MapClientCreateDto()
    {
        CreateMap<ClientCreateDto, Client>();
    }

    private void MapClientRetrieveDto()
    {
        CreateMap<Client, ClientRetrieveDto>();
    }
}