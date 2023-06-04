namespace user_management.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public Task<User?> Create(User user);
    public Task<User?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false);
    public Task<User?> RetrieveByFullNameForExistenceCheck(string fullName);
    public Task<User?> RetrieveByUsernameForExistenceCheck(string username);
    public Task<User?> RetrieveByEmailForExistenceCheck(string email);
    public Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber);
    public Task<List<User>> Retrieve(ObjectId actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false);
    public Task<User?> RetrieveByIdForAuthentication(ObjectId userId);
    public Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id);
    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);
    public Task<User?> RetrieveUserForPasswordChange(string email);
    public Task<User?> RetrieveByClientIdAndCode(ObjectId clientId, string code);
    public Task<User?> RetrieveByRefreshTokenValue(string value);
    public Task<User?> RetrieveByTokenValue(string value);
    public Task<bool?> Login(User user);
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);
    public Task<bool?> Verify(ObjectId id);
    public Task<bool?> ChangePassword(string email, string hashedPassword);
    public Task<bool?> Logout(ObjectId id);
    public Task<bool?> RemoveClient(User user, ObjectId clientId);
    public Task<bool?> RemoveAllClients(User user);
    public Task<bool?> AddToken(User user, ObjectId clientId, string tokenValue, DateTime expirationDate, IClientSessionHandle? session = null);
    public Task<bool?> AddTokenPrivileges(User user, ObjectId clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null);
    public Task<bool?> AddClientById(User user, ObjectId clientId, TokenPrivileges tokenPrivileges, DateTime refreshTokenExpiration, string refreshTokenValue, DateTime codeExpiresAt, string code, string codeChallenge, string codeChallengeMethod);
    public Task<bool?> UpdateUserPrivileges(ObjectId actorId, ObjectId userId, UserPrivileges userPrivileges, bool forClients);
    public Task<bool?> Update(ObjectId actorId, string filtersString, string updatesString, bool forClients = false);
    public Task<bool?> Delete(ObjectId actorId, ObjectId id, bool forClients = false);
}