namespace user_management.Services.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<User?> Create(User user);
    public Task<User?> RetrieveByFullNameForExistenceCheck(string firstName, string middleName, string lastName);
    public Task<User?> RetrieveByUsernameForExistenceCheck(string username);
    public Task<User?> RetrieveByEmailForExistenceCheck(string email);
    public Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber);
    public Task<User?> RetrieveById(ObjectId id);
    public Task<PartialUser?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false);
    public Task<List<PartialUser>> Retrieve(ObjectId actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false);
    public Task<User?> RetrieveByIdForAuthenticationHandling(ObjectId userId);
    public Task<User?> RetrieveByIdForAuthorizationHandling(ObjectId id);
    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);
    public Task<User?> RetrieveUserForPasswordChange(string email);
    public Task<User?> RetrieveUserForUsernameChange(string email);
    public Task<User?> RetrieveUserForEmailChange(string email);
    public Task<User?> RetrieveUserForPhoneNumberChange(string email);
    public Task<User?> RetrieveByClientIdAndCode(ObjectId clientId, string code);
    public Task<User?> RetrieveByRefreshTokenValue(string value);
    public Task<User?> RetrieveByTokenValue(string value);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Login(ObjectId userId);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecretForActivation(string VerificationSecret, string email);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateVerificationSecretForPasswordChange(string VerificationSecret, string email);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Verify(ObjectId id);
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
    public Task<bool?> Logout(ObjectId id);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> RemoveClient(ObjectId userId, ObjectId clientId, ObjectId authorId, bool isClient);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> RemoveAllClients(ObjectId userId, ObjectId authorId, bool isClient);
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> AddToken(ObjectId userId, ObjectId authorId, ObjectId clientId, Token token, IClientSessionHandle? session = null);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> AddTokenPrivilegesToUser(ObjectId userId, ObjectId authorId, ObjectId clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null);
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> AddClientById(ObjectId userId, ObjectId actorId, UserClient userClient);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> UpdateUserPrivileges(ObjectId authorId, ObjectId userId, UserPrivileges userPrivileges);
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Update(ObjectId actorId, string filtersString, string updatesString, bool forClients = false);
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<bool?> Delete(ObjectId actorId, ObjectId id, bool forClients = false);
}