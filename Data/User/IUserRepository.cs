namespace user_management.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public Task<User?> Create(User user);
    public Task<User?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false);
    public Task<User?> RetrieveByFullNameForUniqueCheck(string fullName);
    public Task<User?> RetrieveByUsernameForUniqueCheck(string username);
    public Task<User?> RetrieveByEmailForUniqueCheck(string email);
    public Task<User?> RetrieveByPhoneNumberForUniqueCheck(string phoneNumber);
    public Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id);
    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);
    public Task<User?> RetrieveUserForPasswordChange(string email);
    public Task<User?> RetrieveByTokenValue(string value);
    public Task<bool?> Login(User user);
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);
    public Task<bool?> Verify(ObjectId id);
    public Task<bool?> ChangePassword(string email, string hashedPassword);
}