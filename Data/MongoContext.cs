namespace user_management.Data;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;

public class MongoContext
{
    public bool IsSeeded { get; set; }
    public string ConnectionString { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
    public Collections Collections { get; set; } = null!;

    public static async Task Initialize(IOptions<MongoContext> mongoContextOptions)
    {
        MongoContext mongoContext = mongoContextOptions.Value;

        MongoClient client = new MongoClient(mongoContext.ConnectionString);
        IMongoDatabase database = client.GetDatabase(mongoContext.DatabaseName);
        IMongoCollection<Models.User> userCollection = database.GetCollection<Models.User>(mongoContext.Collections.Users);
        IMongoCollection<Models.Client> clientCollection = database.GetCollection<Models.Client>(mongoContext.Collections.Clients);
        FilterDefinitionBuilder<Models.User> fb = Builders<Models.User>.Filter;

        await ClearDatabaseAsync(database);

        IndexKeysDefinition<Models.Client> clientSecretIndex = Builders<Models.Client>.IndexKeys.Ascending(Models.Client.SECRET);
        await clientCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.Client>(clientSecretIndex, new CreateIndexOptions() { Unique = true }));

        IndexKeysDefinition<Models.User> userEmailIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.EMAIL);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userEmailIndex, new CreateIndexOptions() { Unique = true }));

        IndexKeysDefinition<Models.User> userFullNameIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.FIRST_NAME).Ascending(Models.User.MIDDLE_NAME).Ascending(Models.User.LAST_NAME);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userFullNameIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.And(
                fb.Exists(Models.User.FIRST_NAME),
                fb.Exists(Models.User.MIDDLE_NAME),
                fb.Exists(Models.User.LAST_NAME),
                fb.SizeGt(Models.User.FIRST_NAME, 0),
                fb.SizeGt(Models.User.MIDDLE_NAME, 0),
                fb.SizeGt(Models.User.LAST_NAME, 0)
            )
        }));

        IndexKeysDefinition<Models.User> refreshTokenCodeIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(refreshTokenCodeIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.And(
                fb.Exists(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE),
                fb.SizeGt(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, 0)
            )
        }));

        IndexKeysDefinition<Models.User> refreshTokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.VALUE);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(refreshTokenValueIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.And(
                fb.Exists(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE),
                fb.SizeGt(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, 0)
            )
        }));

        IndexKeysDefinition<Models.User> tokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(tokenValueIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.And(
                fb.Exists(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE),
                fb.SizeGt(Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.CODE, 0)
            )
        }));
    }

    private static async Task ClearDatabaseAsync(IMongoDatabase database)
    {
        foreach (string collection in database.ListCollectionNames().ToList())
        {
            await database.DropCollectionAsync(collection);
        }
    }
}


public class Collections
{
    public string Users { get; set; } = null!;
    public string Clients { get; set; } = null!;
}