namespace user_management.Data.Client;

using MongoDB.Bson;
using user_management.Models;

public interface IClientRepository
{
    public Task<Client> Create(Client client);
    public Task<Client?> RetrieveById(ObjectId id);
    public Task<Client?> RetrieveBySecret(string secret);
    public Task<bool> UpdateRedirectUrl(string redirectUrl, ObjectId id, string hashedSecret);
}