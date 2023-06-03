namespace user_management.Data.Client;

using MongoDB.Bson;
using user_management.Models;

public interface IClientRepository
{
    public Task<Client> Create(Client client);
}