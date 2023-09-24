using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Data;

namespace user_management_integration_tests.Data.Client;

[CollectionDefinition("ClientCollectionTest", DisableParallelization = true)]
public class ClientCollectionTestCollectionDefinition { }

[Collection("ClientCollectionTest")]
public class ClientCollectionTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IMongoCollection<user_management.Models.Client> _clientCollection;
    public static Faker Faker = new("en");
    private MongoClient _mongoClient;

    public ClientCollectionTest(CustomWebApplicationFactory<Program> factory)
    {
        MongoCollections mongoCollections = factory.Services.GetService<MongoCollections>()!;
        _clientCollection = mongoCollections.Clients;
        _mongoClient = factory.Services.GetService<MongoClient>()!;

        _clientCollection.DeleteManyAsync(Builders<user_management.Models.Client>.Filter.Empty).Wait();
    }

    private static user_management.Models.Client TemplateClient() => new user_management.Models.Client()
    {
        Id = ObjectId.GenerateNewId(),
        RedirectUrl = Faker.Internet.Url(),
        Secret = ObjectId.GenerateNewId().ToString(),
        UpdatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
        CreatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(-8))
    };

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<user_management.Models.Client> GenerateClients(int count = 1)
    {
        IEnumerable<user_management.Models.Client> clients = new user_management.Models.Client[] { };
        for (int i = 0; i < count; i++)
        {
            user_management.Models.Client client = TemplateClient();
            int safety = 0;
            do { client = TemplateClient(); safety++; }
            while (safety < 500 && clients.FirstOrDefault<user_management.Models.Client?>(u => u != null && (u.Secret == client.Secret || u.RedirectUrl == client.RedirectUrl)) != null);
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
    public async void RedirectUrlIndex(user_management.Models.Client client1, user_management.Models.Client client2)
    {
        client2.RedirectUrl = client1.RedirectUrl;
        await TestIndex(client1, client2);
    }

    [Theory]
    [MemberData(nameof(TwoClients))]
    public async void SecretIndex(user_management.Models.Client client1, user_management.Models.Client client2)
    {
        client2.Secret = client1.Secret;
        await TestIndex(client1, client2);
    }

    private async Task TestIndex(user_management.Models.Client client1, user_management.Models.Client client2)
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
