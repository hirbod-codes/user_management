namespace user_management.Data;

using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using MongoDB.Bson;
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

    public async Task Initialize()
    {
        MongoClient client = GetMongoClient();
        IMongoDatabase database = client.GetDatabase(DatabaseName);
        IMongoCollection<Models.User> userCollection = database.GetCollection<Models.User>(Collections.Users);
        IMongoCollection<Models.Client> clientCollection = database.GetCollection<Models.Client>(Collections.Clients);

        await ClearDatabaseAsync(database);

        await CreateClientsCollectionIndexes(clientCollection);
        await CreateUsersCollectionIndexes(userCollection);
    }

    private async Task CreateClientsCollectionIndexes(IMongoCollection<Models.Client> clientCollection)
    {
        // Secret
        IndexKeysDefinition<Models.Client> clientSecretIndex = Builders<Models.Client>.IndexKeys.Ascending(Models.Client.SECRET);
        await clientCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.Client>(clientSecretIndex, new CreateIndexOptions() { Unique = true }));

        // RedirectUrl
        IndexKeysDefinition<Models.Client> clientRedirectUrlIndex = Builders<Models.Client>.IndexKeys.Ascending(Models.Client.REDIRECT_URL);
        await clientCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.Client>(clientRedirectUrlIndex, new CreateIndexOptions() { Unique = true }));
    }

    private async Task CreateUsersCollectionIndexes(IMongoCollection<Models.User> userCollection)
    {
        FilterDefinitionBuilder<Models.User> fb = Builders<Models.User>.Filter;

        // Email
        IndexKeysDefinition<Models.User> userEmailIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.EMAIL);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userEmailIndex, new CreateIndexOptions<Models.User>() { Unique = true }));

        // Username
        IndexKeysDefinition<Models.User> userUsernameIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.USERNAME);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userUsernameIndex, new CreateIndexOptions<Models.User>() { Unique = true, }));

        // PhoneNumber
        string userPhoneNumberIndexField = Models.User.PHONE_NUMBER;
        IndexKeysDefinition<Models.User> userPhoneNumberIndex = Builders<Models.User>.IndexKeys.Ascending(userPhoneNumberIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userPhoneNumberIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.Type(userPhoneNumberIndexField, BsonType.String)
        }));

        // UpdatedAt
        IndexKeysDefinition<Models.User> userUpdatedAtIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.UPDATED_AT);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userUpdatedAtIndex));

        // CreateAt
        IndexKeysDefinition<Models.User> userCreateAtIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.CREATED_AT);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userCreateAtIndex));

        // FirstName.MiddleName.LastName
        IndexKeysDefinition<Models.User> userFullNameIndex = Builders<Models.User>.IndexKeys.Ascending(Models.User.FIRST_NAME).Ascending(Models.User.MIDDLE_NAME).Ascending(Models.User.LAST_NAME);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(userFullNameIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.And(
                fb.Type(Models.User.FIRST_NAME, BsonType.String),
                fb.Type(Models.User.MIDDLE_NAME, BsonType.String),
                fb.Type(Models.User.LAST_NAME, BsonType.String)
            )
        }));

        // AuthorizingClient.Code
        string AuthorizingClientCodeIndexField = Models.User.AUTHORIZING_CLIENT + "." + AuthorizingClient.CODE;
        IndexKeysDefinition<Models.User> AuthorizingClientCodeIndex = Builders<Models.User>.IndexKeys.Ascending(AuthorizingClientCodeIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(AuthorizingClientCodeIndex, new CreateIndexOptions<Models.User>()
        {
            Unique = true,
            PartialFilterExpression = fb.Type(AuthorizingClientCodeIndexField, BsonType.String)
        }));

        // Clients.RefreshToken.Value
        string refreshTokenValueIndexField = Models.User.CLIENTS + "." + UserClient.REFRESH_TOKEN + "." + RefreshToken.VALUE;
        IndexKeysDefinition<Models.User> refreshTokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(refreshTokenValueIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(refreshTokenValueIndex, new CreateIndexOptions<Models.User>() { Unique = true, Sparse = true }));

        // Clients.Token.Value
        string tokenValueIndexField = Models.User.CLIENTS + "." + UserClient.TOKEN + "." + Token.VALUE;
        IndexKeysDefinition<Models.User> tokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(tokenValueIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(tokenValueIndex, new CreateIndexOptions<Models.User>() { Unique = true, Sparse = true }));
    }

    public MongoClient GetMongoClient() => new MongoClient(new MongoClientSettings()
    {
        Credential = MongoCredential.CreateMongoX509Credential(Username),
        SslSettings = new SslSettings
        {
            ClientCertificates = new List<X509Certificate>()
                {
                    new X509Certificate2(CertificateP12, "") {}
                },
            CheckCertificateRevocation = false,
            EnabledSslProtocols = SslProtocols.Tls12
        },
        AllowInsecureTls = true,
        UseTls = true,
        Server = new MongoServerAddress(Host, Port),
        Scheme = ConnectionStringScheme.MongoDB,
        WriteConcern = WriteConcern.WMajority,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.Primary
    });

    public static async Task ClearDatabaseAsync(IMongoDatabase database)
    {
        foreach (string collection in database.ListCollectionNames().ToList())
            await database.DropCollectionAsync(collection);
    }
}


public class Collections
{
    public string Users { get; set; } = null!;
    public string Clients { get; set; } = null!;
}