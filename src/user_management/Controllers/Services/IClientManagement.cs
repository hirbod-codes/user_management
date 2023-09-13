using MongoDB.Bson;
using user_management.Models;

namespace user_management.Controllers.Services;

public interface IClientManagement
{
    /// <exception cref="user_management.Services.Client.RegistrationFailure"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<(Models.Client client, string? notHashedSecret)> Register(Models.Client client);

    /// <exception cref="System.ArgumentException"></exception>
    public Task<Client?> RetrieveClientPublicInfo(string id);

    /// <exception cref="System.ArgumentException"></exception>
    public Task<Client?> RetrieveBySecret(string secret);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task UpdateRedirectUrl(string clientId, string clientSecret, string redirectUrl);

    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    public Task DeleteBySecret(string clientId, string secret);

    /// <exception cref="user_management.Services.OperationException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<string> UpdateExposedClient(ObjectId clientId, string secret);
}