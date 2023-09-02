namespace user_management.Services.Client;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IClientRepository
{
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<Client> Create(Client client, IClientSessionHandle? session = null);
    public Task<Client?> RetrieveById(ObjectId clientId);
    public Task<Client?> RetrieveBySecret(string secret);
    public Task<Client?> RetrieveByIdAndSecret(ObjectId clientId, string hashedSecret);
    public Task<Client?> RetrieveByIdAndRedirectUrl(ObjectId clientId, string redirectUrl);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> UpdateRedirectUrl(string redirectUrl, ObjectId clientId, string hashedSecret);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteBySecret(string secret, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteById(ObjectId clientId, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> ClientExposed(ObjectId clientId, string newSecret, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> ClientExposed(Client client, string newSecret, IClientSessionHandle? session = null);
}