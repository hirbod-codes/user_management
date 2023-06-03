namespace user_management.Data.User;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;
using user_management.Data;
using MongoDB.Bson;
using user_management.Data.Logics.Filter;
using user_management.Data.Logics.Update;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _userCollection;

    public UserRepository(IOptions<MongoContext> MongoContext)
    {
        MongoClient mongoClient = new MongoClient(MongoContext.Value.ConnectionString);

        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(MongoContext.Value.DatabaseName);

        _userCollection = mongoDatabase.GetCollection<User>(MongoContext.Value.Collections.Users);
    }

    public async Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id)
    {
        User? user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", id))).FirstOrDefault<User?>();
        if (user == null)
            return null;

        return user.UserPrivileges;
    }

    public async Task<User?> RetrieveByTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE, value))).FirstOrDefault<User?>();

}