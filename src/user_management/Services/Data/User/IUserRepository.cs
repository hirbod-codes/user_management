namespace user_management.Services.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public string GenerateId();

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<User?> Create(User user);

    /// <exception cref="ArgumentException"></exception>
    public Task<User?> RetrieveByFullNameForExistenceCheck(string? firstName, string? middleName, string? lastName);

    public Task<User?> RetrieveByUsernameForExistenceCheck(string username);

    public Task<User?> RetrieveByEmailForExistenceCheck(string email);

    public Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber);

    public Task<User?> RetrieveById(string id);

    public Task<PartialUser?> RetrieveById(string actorId, string id, bool forClients = false);

    public Task<List<PartialUser>> Retrieve(string actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false);

    public Task<User?> RetrieveByIdForAuthenticationHandling(string userId);

    public Task<User?> RetrieveByIdForAuthorizationHandling(string id);

    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);

    public Task<User?> RetrieveUserForPasswordChange(string email);

    public Task<User?> RetrieveUserForUsernameChange(string email);

    public Task<User?> RetrieveUserForEmailChange(string email);

    public Task<User?> RetrieveUserForPhoneNumberChange(string email);

    public Task<User?> RetrieveByClientIdAndCode(string clientId, string code);

    public Task<User?> RetrieveByRefreshTokenValue(string value);

    public Task<User?> RetrieveByTokenValue(string value);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Login(string userId);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecretForActivation(string VerificationSecret, string email);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecretForPasswordChange(string VerificationSecret, string email);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Verify(string id);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<User?> RetrieveUserForUnverifiedEmailChange(string email);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ChangePassword(string email, string hashedPassword);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ChangeUsername(string email, string username);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ChangePhoneNumber(string email, string phoneNumber);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> ChangeEmail(string email, string newEmail);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Logout(string id);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> RemoveClient(string userId, string clientId, string authorId, bool isClient);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> RemoveAllClients(string userId, string authorId, bool isClient);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> AddTokenPrivilegesToUser(string userId, string authorId, string clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateAuthorizingClient(string userId, AuthorizingClient authorizingClient);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> AddAuthorizedClient(string userId, AuthorizedClient authorizedClient, IClientSessionHandle? session = null);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateToken(string userId, string clientObjectId, Token token);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateUserPrivileges(string authorId, string userId, UserPermissions userPrivileges);

    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Update(string actorId, string filtersString, string updatesString, bool forClients = false);

    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Delete(string actorId, string id, bool forClients = false);
}
