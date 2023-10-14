using System.Reflection;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using user_management.Data;
using user_management.Data.Client;
using user_management.Services.Data;

namespace user_management_integration_tests.Data.Client;

[CollectionDefinition("ClientRepositoryTest", DisableParallelization = true)]
public class ClientRepositoryTestCollectionDefinition { }

[Collection("ClientRepositoryTest")]
public class ClientRepositoryTest : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly MongoCollections _mongoCollections;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly IMongoCollection<user_management.Models.Client> _clientCollection;
    private readonly ClientRepository _clientRepository;
    public static Faker Faker = new("en");

    public ClientRepositoryTest(CustomWebApplicationFactory<Program> factory)
    {
        _mongoCollections = factory.Services.GetService<MongoCollections>()!;
        _mongoDatabase = factory.Services.GetService<IMongoDatabase>()!;

        _clientCollection = _mongoCollections.Clients;
        _clientRepository = new(factory.Services.GetService<IMongoClient>()!, _mongoCollections);
    }

    public Task InitializeAsync() => _mongoCollections.ClearCollections(_mongoDatabase);

    public Task DisposeAsync() => Task.CompletedTask;

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<user_management.Models.Client> GenerateClients(int count = 1)
    {
        IEnumerable<user_management.Models.Client> clients = new user_management.Models.Client[] { };

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
    public async void Create(user_management.Models.Client client1, user_management.Models.Client client2)
    {
        try
        {
            user_management.Models.Client? createdClient = await _clientRepository.Create(client1);

            Assert.NotNull(createdClient);
            user_management.Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", createdClient.Id))).FirstOrDefault<user_management.Models.Client?>();
            Assert.NotNull(retrievedClient);
            AssertFieldsExpectedValues(client1, retrievedClient, new() { { "_id", retrievedClient.Id }, { user_management.Models.Client.UPDATED_AT, retrievedClient.UpdatedAt }, { user_management.Models.Client.CREATED_AT, retrievedClient.CreatedAt } });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client1.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client1.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client2);

        client1.Secret = client2.Secret;
        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _clientRepository.Create(client1));
        }
        finally
        {
            await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client2.Id));
            await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client1.Id));
        }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client1.Id))).FirstOrDefault<user_management.Models.Client?>());
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client2.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveById(user_management.Models.Client client)
    {
        user_management.Models.Client? retrievedClient = null;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            retrievedClient = await _clientRepository.RetrieveById(client.Id);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        retrievedClient = await _clientRepository.RetrieveById(client.Id);

        Assert.Null(retrievedClient);

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveBySecret(user_management.Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveBySecret(client.Secret);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveBySecret("client.Secret");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveByIdAndSecret(user_management.Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndSecret(client.Id, client.Secret);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndSecret(client.Id, "client.Secret");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveByIdAndRedirectUrl(user_management.Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndRedirectUrl(client.Id, client.RedirectUrl);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            user_management.Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndRedirectUrl(client.Id, "client.RedirectUrl");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(TwoClients))]
    public async void UpdateRedirectUrl(user_management.Models.Client client, user_management.Models.Client client2)
    {
        string newRedirectUrl = Faker.Internet.Url();

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            bool? result = await _clientRepository.UpdateRedirectUrl(newRedirectUrl, client.Id, client.Secret);

            Assert.True(result);
            user_management.Models.Client retrievedClient = (await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).First();
            AssertFieldsExpectedValues(client, retrievedClient, new() { { user_management.Models.Client.REDIRECT_URL, newRedirectUrl }, { user_management.Models.Client.UPDATED_AT, retrievedClient.UpdatedAt } });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);
        await _clientCollection.InsertOneAsync(client2);

        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _clientRepository.UpdateRedirectUrl(client2.RedirectUrl, client.Id, client.Secret));
        }
        finally { await _clientCollection.DeleteManyAsync(Builders<user_management.Models.Client>.Filter.Empty); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client2.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void DeleteBySecret(user_management.Models.Client client)
    {
        bool? result;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            result = await _clientRepository.DeleteBySecret(client.Secret);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        result = await _clientRepository.DeleteBySecret(client.Secret);
        Assert.False(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void DeleteById(user_management.Models.Client client)
    {
        bool? result;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            result = await _clientRepository.DeleteById(client.Id);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        result = await _clientRepository.DeleteById(client.Id);
        Assert.False(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void ClientExposed(user_management.Models.Client client)
    {
        bool? result;
        string newHashedSecret = "newHashedSecret";

        // Success
        await _clientCollection.InsertOneAsync(client);

        user_management.Models.Client oldClient = (await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).First();

        try
        {
            result = await _clientRepository.ClientExposed(client, newHashedSecret);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", oldClient.Id))).FirstOrDefault<user_management.Models.Client?>());
            user_management.Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq(user_management.Models.Client.SECRET, newHashedSecret))).FirstOrDefault<user_management.Models.Client?>();
            Assert.NotNull(retrievedClient);
            Assert.NotNull(retrievedClient.TokensExposedAt);
            Assert.NotEqual(oldClient.Id.ToString(), retrievedClient.Id.ToString());
            Assert.Equal(oldClient.RedirectUrl, retrievedClient.RedirectUrl);
            Assert.Equal(newHashedSecret, retrievedClient.Secret);
            Assert.Equal<int>(oldClient.ExposedCount + 1, retrievedClient.ExposedCount);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        result = await _clientRepository.ClientExposed(client, newHashedSecret);
        Assert.Null(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void ClientExposed_byId(user_management.Models.Client client)
    {
        bool? result;
        string newHashedSecret = "newHashedSecret";
        string hashedSecret = client.Secret;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            result = await _clientRepository.ClientExposed(client.Id, hashedSecret, newHashedSecret);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
            user_management.Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq(user_management.Models.Client.SECRET, newHashedSecret))).FirstOrDefault<user_management.Models.Client?>();
            Assert.NotNull(retrievedClient);
            Assert.NotNull(retrievedClient.TokensExposedAt);
            Assert.NotEqual(client.Id.ToString(), retrievedClient.Id.ToString());
            Assert.Equal(client.RedirectUrl, retrievedClient.RedirectUrl);
            Assert.Equal(newHashedSecret, retrievedClient.Secret);
            Assert.Equal<int>(client.ExposedCount + 1, retrievedClient.ExposedCount);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());

        // Failure
        result = await _clientRepository.ClientExposed(client.Id, hashedSecret, newHashedSecret);
        Assert.Null(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<user_management.Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<user_management.Models.Client?>());
    }

    private static void AssertFieldsExpectedValues(object oldObject, object newObject, Dictionary<string, object?>? changedFields = null)
    {
        if (oldObject.GetType().FullName != newObject.GetType().FullName) throw new ArgumentException();
        if (changedFields == null) changedFields = new() { };

        Func<PropertyInfo, string> GetKeyFromProperty = p =>
        {
            CustomAttributeData ca = p.CustomAttributes.First(c => c != null && (c.AttributeType == typeof(BsonIdAttribute) || c.AttributeType == typeof(BsonElementAttribute)));

            string key = "";
            if (ca.AttributeType == typeof(BsonIdAttribute))
                key = "_id";
            else if (ca.AttributeType == typeof(BsonElementAttribute))
                key = (ca.ConstructorArguments[0].Value as string)!;

            return key;
        };

        Func<object, List<PropertyInfo>> GetProperties = o =>
        {
            return o
            .GetType()
            .GetProperties()
            .Where(p => p.CustomAttributes.FirstOrDefault(c => c != null && (c.AttributeType == typeof(BsonElementAttribute) || c.AttributeType == typeof(BsonIdAttribute))) != null)
            .ToList();
        };

        List<PropertyInfo> newObjectProperties = GetProperties(newObject);
        for (int i = 0; i < newObjectProperties.Count(); i++)
        {
            PropertyInfo p = newObjectProperties[i];
            object? newObjectValue = p.GetValue(newObject);

            CustomAttributeData ca = p.CustomAttributes.First(c => c != null && (c.AttributeType == typeof(BsonIdAttribute) || c.AttributeType == typeof(BsonElementAttribute)));

            PropertyInfo? oldObjectProperty = oldObject.GetType().GetProperty(p.Name);
            Assert.NotNull(oldObjectProperty);
            object? oldObjectValue = oldObjectProperty.GetValue(oldObject);

            string key = GetKeyFromProperty(p);
            var newValue = changedFields.GetValueOrDefault(key);

            if (key != "verification_secret_updated_at") continue;

            try
            {
                if (changedFields.ContainsKey(key))
                {
                    if (key == "_id")
                        Assert.True(newObjectValue!.ToString() == newValue!.ToString());
                    else
                    {
                        Assert.True(AreTwoValueEqual(newObjectValue, newValue));
                        Assert.False(AreTwoValueEqual(newObjectValue, oldObjectValue));
                    }
                }
                else if (key == "_id")
                    Assert.True(newObjectValue!.ToString() == oldObjectValue!.ToString());
                else
                    Assert.True(AreTwoValueEqual(oldObjectValue, newObjectValue));
            }
            catch (System.Exception) { throw; }
        }
    }

    public static bool AreTwoValueEqual(object? v1, object? v2)
    {
        if (v1 == null && v2 == null) return true;
        else if (v1 == null || v2 == null) return false;

        IEnumerable<object>? iterableObject = v1 as IEnumerable<object>;
        if (iterableObject != null)
        {
            for (int j = 0; j < iterableObject.Count(); j++)
                if (iterableObject.ElementAt(j).GetType() == typeof(DateTime) && (
                    (v2 as IEnumerable<object>) == null ||
                    Math.Floor((decimal)((DateTime)iterableObject.ElementAt(j)).Ticks / 10000) != Math.Floor((decimal)((DateTime)(v2 as IEnumerable<object>)!.ElementAt(j)).Ticks / 10000))
                )
                    return false;
                else if (iterableObject.ElementAt(j).GetType() != typeof(DateTime) && ((v2 as IEnumerable<object>) == null || !iterableObject.ElementAt(j).Equals((v2 as IEnumerable<object>)!.ElementAt(j))))
                    return false;

            return true;
        }
        else return v1.GetType() == typeof(DateTime) ? Math.Floor((decimal)((DateTime)v1).Ticks / 10000) == Math.Floor((decimal)((DateTime)v2).Ticks / 10000) : v2.Equals(v1);
    }
}
