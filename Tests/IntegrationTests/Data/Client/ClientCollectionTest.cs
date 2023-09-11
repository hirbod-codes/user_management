using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Data;
using user_management.Data.Client;
using Xunit;

namespace user_management.Tests.IntegrationTests.Data.Client;

public class ClientCollectionTest
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<Models.Client> _clientCollection;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ClientRepository _clientRepository;
    public static Faker Faker = new("en");

    public ClientCollectionTest()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { EnvironmentName = "Development" });

        builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
        MongoContext mongoContext = new();
        builder.Configuration.GetSection("MongoDB").Bind(mongoContext);

        _mongoClient = mongoContext.GetMongoClient();
        _mongoDatabase = _mongoClient.GetDatabase(mongoContext.DatabaseName);
        _clientCollection = _mongoDatabase.GetCollection<Models.Client>(mongoContext.Collections.Clients);

        _clientRepository = new ClientRepository(mongoContext);

        mongoContext.Initialize().Wait();
    }

    private static Models.Client TemplateClient() => new Models.Client()
    {
        Id = ObjectId.GenerateNewId(),
        RedirectUrl = Faker.Internet.Url(),
        Secret = ObjectId.GenerateNewId().ToString(),
        UpdatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
        CreatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(-8))
    };

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<Models.Client> GenerateClients(int count = 1)
    {
        IEnumerable<Models.Client> clients = new Models.Client[] { };
        for (int i = 0; i < count; i++)
        {
            Models.Client client = TemplateClient();
            int safety = 0;
            do { client = TemplateClient(); safety++; }
            while (safety < 500 && clients.FirstOrDefault<Models.Client?>(u => u != null && (u.Secret == client.Secret || u.RedirectUrl == client.RedirectUrl)) != null);
            if (safety >= 500) throw new Exception("While loop safety triggered at GenerateClients private method of ClientRepositoryTests.");

            clients = clients.Append(client);
        }

        return clients;
    }

    public static IEnumerable<object?[]> OneClient =>
        new List<object?[]>
        {
            new object?[] {
                GenerateClients().ElementAt(0)
            },
        };

    public static IEnumerable<object?[]> TwoClients =>
        new List<object?[]>
        {
            GenerateClients(2).ToArray(),
        };

    public static IEnumerable<object?[]> ManyClients =>
        new List<object?[]>
        {
            new object?[] {
                GenerateClients(20).ToArray()
            },
        };

    [Theory]
    [MemberData(nameof(TwoClients))]
    public async void RedirectUrlIndex(Models.Client client1, Models.Client client2)
    {
        client2.RedirectUrl = client1.RedirectUrl;
        await TestIndex(client1, client2);
    }

    [Theory]
    [MemberData(nameof(TwoClients))]
    public async void SecretIndex(Models.Client client1, Models.Client client2)
    {
        client2.Secret = client1.Secret;
        await TestIndex(client1, client2);
    }

    private async Task TestIndex(Models.Client client1, Models.Client client2)
    {
        IClientSessionHandle? session = null;
        try
        {
            session = await _mongoClient.StartSessionAsync();

            session.StartTransaction(new(writeConcern: WriteConcern.WMajority));

            await _clientCollection.InsertOneAsync(session, client1);

            await Assert.ThrowsAsync<MongoWriteException>(async () => await _clientCollection.InsertOneAsync(session, client2));

            await session.AbortTransactionAsync();
        }
        catch (Exception) { if (session != null) await session.AbortTransactionAsync(); throw; }
        finally { if (session != null) session.Dispose(); }
    }
}