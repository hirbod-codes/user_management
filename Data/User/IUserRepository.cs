namespace user_management.Data.User;

using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;

public interface IUserRepository
{
    public Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id);
    public Task<User?> RetrieveByTokenValue(string value);
}