namespace user_management.Data.MongoDB;

using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using global::MongoDB.Bson;
using global::MongoDB.Driver;
using global::MongoDB.Driver.Core.Configuration;
using user_management.Models;
using user_management.Data.MongoDB.Client;
using user_management.Data.MongoDB.User;
using user_management.Services.Data.Client;
using user_management.Services.Data.User;

public class ShardedMongoContext
{
    public string Username { get; set; } = null!;
    public string CertificateP12 { get; set; } = null!;
    public string Host { get; set; } = null!;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = null!;

    public async Task Initialize(MongoCollections mongoCollections, IMongoDatabase mongoDatabase)
    {
        await ClearDatabase(mongoCollections, mongoDatabase);

        await CreateClientsCollectionIndexes(mongoCollections.Clients);
        await CreateUsersCollectionIndexes(mongoCollections.Users);
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
        string refreshTokenValueIndexField = Models.User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.REFRESH_TOKEN + "." + RefreshToken.VALUE;
        IndexKeysDefinition<Models.User> refreshTokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(refreshTokenValueIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(refreshTokenValueIndex, new CreateIndexOptions<Models.User>() { Unique = true, Sparse = true }));

        // Clients.Token.Value
        string tokenValueIndexField = Models.User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.TOKEN + "." + Token.VALUE;
        IndexKeysDefinition<Models.User> tokenValueIndex = Builders<Models.User>.IndexKeys.Ascending(tokenValueIndexField);
        await userCollection.Indexes.CreateOneAsync(new CreateIndexModel<Models.User>(tokenValueIndex, new CreateIndexOptions<Models.User>() { Unique = true, Sparse = true }));
    }

    public MongoClient GetClient() => new(settings: new()
    {
        Credential = MongoCredential.CreateMongoX509Credential(Username),
        SslSettings = new SslSettings
        {
            ClientCertificates = new List<X509Certificate>() { new X509Certificate2(CertificateP12.StartsWith('/') ? CertificateP12 : Program.RootPath + "/../../" + CertificateP12, "") },
            CheckCertificateRevocation = false,
            EnabledSslProtocols = SslProtocols.Tls12
        },
        UseTls = true,
        Server = new MongoServerAddress(Host, Port),
        Scheme = ConnectionStringScheme.MongoDB,
        WriteConcern = WriteConcern.WMajority,
        ReadConcern = ReadConcern.Majority,
        ReadPreference = ReadPreference.Primary
    });

    public async Task ClearDatabase(MongoCollections mongoCollections, IMongoDatabase mongoDatabase) => await mongoCollections.ClearCollections(mongoDatabase);

    public static void ConfigureShardedMongodb(IServiceCollection services, IConfiguration configuration)
    {
        ShardedMongoContext dbContext = new();

        configuration.GetSection("DB_OPTIONS").Bind(dbContext);
        services.AddSingleton(dbContext);

        MongoClient dbClient = dbContext.GetClient();
        services.AddSingleton<IMongoClient>(dbClient);

        services.AddSingleton(dbClient.GetDatabase(dbContext.DatabaseName));

        MongoCollections mongoCollections = new();
        mongoCollections.InitializeCollections(dbClient.GetDatabase(dbContext.DatabaseName));
        services.AddSingleton(mongoCollections);

        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<IClientRepository, ClientRepository>();
    }
}
