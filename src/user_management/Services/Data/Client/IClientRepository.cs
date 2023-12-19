namespace user_management.Services.Client;

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IClientRepository
{
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<Client> Create(Client client, IClientSessionHandle? session = null);
    public Task<Client?> RetrieveById(string clientId);
    public Task<Client?> RetrieveBySecret(string secret);
    public Task<Client?> RetrieveByIdAndSecret(string clientId, string hashedSecret);
    public Task<Client?> RetrieveByIdAndRedirectUrl(string clientId, string redirectUrl);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> UpdateRedirectUrl(string redirectUrl, string clientId, string hashedSecret);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteBySecret(string secret, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteById(string clientId, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposed(string clientId, string hashedSecret, string newHashedSecret, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposed(Client client, string newHashedSecret, IClientSessionHandle? session = null);
}
