namespace user_management.Services.Data.Client;

using System.Collections.Generic;
using user_management.Models;

public interface IClientRepository : IAtomic
{
    public Task<IEnumerable<Client>> RetrieveFirstPartyClients();

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<Client> Create(Client client);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<Client> CreateWithTransaction(Client client);
    public Task<Client?> RetrieveById(string clientId);
    public Task<Client?> RetrieveBySecret(string hashedSecret);
    public Task<Client?> RetrieveByIdAndSecret(string clientId, string hashedSecret);
    public Task<Client?> RetrieveByIdAndRedirectUrl(string clientId, string redirectUrl);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> UpdateRedirectUrl(string redirectUrl, string clientId, string hashedSecret);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteBySecret(string hashedSecret);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteBySecretWithTransaction(string hashedSecret);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteById(string clientId);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool> DeleteByIdWithTransaction(string clientId);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposed(string clientId, string hashedSecret, string newHashedSecret);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposedWithTransaction(string clientId, string hashedSecret, string newHashedSecret);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposed(Client client, string newHashedSecret);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ClientExposedWithTransaction(Client client, string newHashedSecret);
}
