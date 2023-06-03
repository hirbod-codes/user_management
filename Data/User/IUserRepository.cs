namespace user_management.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public Task<User?> Create(User user);
    public Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id);
    public Task<User?> RetrieveUserByLoginCredentials(string? email, string? username);
    public Task<User?> RetrieveByTokenValue(string value);
    public Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email);
    public Task<bool?> Verify(ObjectId id);
}