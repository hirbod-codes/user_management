using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using user_management.Data;

namespace user_management_integration_tests.Data.Client;

[CollectionDefinition("ClientCollectionTest", DisableParallelization = true)]
public class ClientCollectionTestCollectionDefinition { }

[Collection("ClientCollectionTest")]
public class ClientCollectionTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IMongoCollection<user_management.Models.Client> _clientCollection;
    private static readonly Faker _faker = new("en");
    private readonly IMongoClient _mongoClient;

    public ClientCollectionTest(CustomWebApplicationFactory<Program> factory)
    {
        MongoCollections mongoCollections = factory.Services.GetService<MongoCollections>()!;
        IMongoDatabase mongoDatabase = factory.Services.GetService<IMongoDatabase>()!;
        mongoCollections.ClearCollections(mongoDatabase).Wait();

        _mongoClient = factory.Services.GetService<IMongoClient>()!;

        _clientCollection = mongoCollections.Clients;
    }

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<user_management.Models.Client> GenerateClients(int count = 1)
    {
        IEnumerable<user_management.Models.Client> clients = Array.Empty<user_management.Models.Client>();

        for (int i = 0; i < count; i++)
            clients = clients.Append(user_management.Models.Client.FakeClient(out string secret, clients));

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
