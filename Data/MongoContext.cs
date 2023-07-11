namespace user_management.Data;

using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using user_management.Models;

public class MongoContext
{
    public string Username { get; set; } = null!;
    public string CaPem { get; set; } = null!;
    public string CertificateP12 { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = null!;
    public Collections Collections { get; set; } = null!;

    public static async Task Initialize(IOptions<MongoContext> mongoContextOptions)
    {
        MongoContext mongoContext = mongoContextOptions.Value;

        MongoClient client = MongoContext.GetMongoClient(mongoContext);
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

    public static MongoClient GetMongoClient(MongoContext mongoContext) => new MongoClient(new MongoClientSettings()
    {
        Credential = MongoCredential.CreateMongoX509Credential(mongoContext.Username),
        SslSettings = new SslSettings
        {
            ClientCertificates = new List<X509Certificate>()
                {
                    new X509Certificate2(mongoContext.CertificateP12, "") {}
                },
            CheckCertificateRevocation = false,
            EnabledSslProtocols = SslProtocols.Tls12
        },
        AllowInsecureTls = true,
        UseTls = true,
        Server = new MongoServerAddress(mongoContext.Host, mongoContext.Port),
        Scheme = ConnectionStringScheme.MongoDB,
        WriteConcern = WriteConcern.WMajority,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.Primary
    });

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