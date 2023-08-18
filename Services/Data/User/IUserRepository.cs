namespace user_management.Services.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public Task<User?> Create(User user);
    public Task<User?> RetrieveById(ObjectId id);
    public Task<User?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false);
    public Task<User?> RetrieveByFullNameForExistenceCheck(string firstName, string middleName, string lastName);
    public Task<User?> RetrieveByUsernameForExistenceCheck(string username);
    public Task<User?> RetrieveByEmailForExistenceCheck(string email);
    public Task<User?> RetrieveByPhoneNumberForExistenceCheck(string phoneNumber);
    public Task<List<User>> Retrieve(ObjectId actorId, string logicsString, int limit, int iteration, string? sortBy, bool ascending = true, bool forClients = false);
    public Task<User?> RetrieveByIdForAuthentication(ObjectId userId);
    public Task<User?> RetrieveByIdForAuthorization(ObjectId id);
    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);
    public Task<User?> RetrieveUserForPasswordChange(string email);
    public Task<User?> RetrieveUserForUsernameChange(string email);
    public Task<User?> RetrieveUserForEmailChange(string email);
    public Task<User?> RetrieveUserForPhoneNumberChange(string email);
    public Task<User?> RetrieveByClientIdAndCode(ObjectId clientId, string code);
    public Task<User?> RetrieveByRefreshTokenValue(string value);
    public Task<User?> RetrieveByTokenValue(string value);
    public Task<bool?> Login(User user);
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);
    public Task<bool?> UpdateVerificationSecretForActivation(string VerificationSecret, string email);
    public Task<bool?> UpdateVerificationSecretForPasswordChange(string VerificationSecret, string email);
    public Task<bool?> Verify(ObjectId id);
    public Task<bool?> ChangePassword(string email, string hashedPassword);
    public Task<bool?> ChangeUsername(string email, string username);
    public Task<bool?> ChangePhoneNumber(string email, string phoneNumber);
    public Task<bool?> ChangeEmail(string email, string newEmail);
    public Task<bool?> Logout(ObjectId id);
    public Task<bool?> RemoveClient(User user, ObjectId clientId, ObjectId authorId, bool isClient);
    public Task<bool?> RemoveAllClients(User user, ObjectId authorId, bool isClient);
    public Task<bool?> AddToken(User user, ObjectId clientId, string tokenValue, DateTime expirationDate, IClientSessionHandle? session = null);
    public Task<bool?> AddTokenPrivileges(User user, ObjectId clientId, TokenPrivileges tokenPrivileges, IClientSessionHandle? session = null);
    public Task<bool?> AddClientById(User user, ObjectId clientId, ObjectId actorId, bool forClients, TokenPrivileges tokenPrivileges, DateTime refreshTokenExpiration, string refreshTokenValue, DateTime codeExpiresAt, string code, string codeChallenge, string codeChallengeMethod);
    public Task<bool?> UpdateUserPrivileges(User author);
    public Task<bool?> Update(ObjectId actorId, string filtersString, string updatesString, bool forClients = false);
    public Task<bool?> Delete(ObjectId actorId, ObjectId id, bool forClients = false);
}