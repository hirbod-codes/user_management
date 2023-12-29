using MongoDB.Driver;

namespace user_management.Data.MongoDB;

public class MongoCollections
{
    public const string USERS = "users";
    public const string CLIENTS = "clients";
    public IMongoCollection<Models.User> Users { get; set; } = null!;
    public IMongoCollection<Models.PartialUser> PartialUsers { get; set; } = null!;
    public IMongoCollection<Models.Client> Clients { get; set; } = null!;

    public void InitializeCollections(IMongoDatabase dbDatabase)
    {
        Users = dbDatabase.GetCollection<Models.User>(USERS);
        PartialUsers = dbDatabase.GetCollection<Models.PartialUser>(USERS);
        Clients = dbDatabase.GetCollection<Models.Client>(CLIENTS);
    }

    public async Task ClearCollections(IMongoDatabase dbDatabase)
    {
        await Users.DeleteManyAsync(Builders<Models.User>.Filter.Empty);
        await Clients.DeleteManyAsync(Builders<Models.Client>.Filter.Empty);
    }
}
