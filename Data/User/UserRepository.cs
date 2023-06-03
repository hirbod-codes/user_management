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

    public async Task<User?> Create(User user)
    {
        user.Id = ObjectId.GenerateNewId();

        DateTime dt = DateTime.UtcNow;
        user.UpdatedAt = dt;
        user.CreatedAt = dt;

        user.UserPrivileges = User.GetDefaultUserPrivileges((ObjectId)user.Id);

        await _userCollection.InsertOneAsync(user);

        return user;
    }

    public async Task<User?> RetrieveByFullNameForUniqueCheck(string fullName) => fullName.Split("-").Length != 3 ? null : (await _userCollection.FindAsync(
        Builders<User>.Filter.And(
            Builders<User>.Filter.Eq(User.FIRST_NAME, fullName.Split("-")[0]),
            Builders<User>.Filter.Eq(User.MIDDLE_NAME, fullName.Split("-")[1]),
            Builders<User>.Filter.Eq(User.LAST_NAME, fullName.Split("-")[2])
        ))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveById(ObjectId actorId, ObjectId id, bool forClients = false) => (await _userCollection.FindAsync(Builders<User>.Filter.And(Builders<User>.Filter.Eq("_id", id), GetReaderFilterDefinition(actorId, forClients)))).FirstOrDefault<User?>();

    public async Task<UserPrivileges?> RetrieveByIdForAuthorization(ObjectId id)
    {
        User? user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", id))).FirstOrDefault<User?>();
        if (user == null)
            return null;

        return user.UserPrivileges;
    }

    public async Task<User?> RetrieveUserByLoginCredentials(string? email, string? username) => (await _userCollection.FindAsync(Builders<User>.Filter.Or(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Filter.Eq(User.USERNAME, username)))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveUserForPasswordChange(string email) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.EMAIL, email))).FirstOrDefault<User?>();

    public async Task<User?> RetrieveByTokenValue(string value) => (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE, value))).FirstOrDefault<User?>();

    public async Task<bool?> UpdateVerificationSecret(string VerificationSecret, string email)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set(User.VERIFICATION_SECRET, VerificationSecret).Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> Verify(ObjectId id)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq("_id", id), Builders<User>.Update.Set<bool>(User.IS_VERIFIED, true).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

    public async Task<bool?> ChangePassword(string email, string hashedPassword)
    {
        UpdateResult result = await _userCollection.UpdateOneAsync(Builders<User>.Filter.Eq(User.EMAIL, email), Builders<User>.Update.Set<string>(User.PASSWORD, hashedPassword).Set(User.UPDATED_AT, DateTime.UtcNow));

        if (result.IsAcknowledged && result.MatchedCount == 0)
            return null;

        return result.IsAcknowledged && result.MatchedCount == 1 && result.ModifiedCount == 1;
    }

}