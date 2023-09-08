using System.Reflection;
using Bogus;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using user_management.Data;
using user_management.Data.Client;
using user_management.Services.Data;
using Xunit;

namespace user_management.Tests.IntegrationTests.Data.Client;

public class ClientRepositoryTest
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<Models.Client> _clientCollection;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly ClientRepository _clientRepository;
    public static Faker Faker = new("en");

    public ClientRepositoryTest()
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
    public async void Create(Models.Client client1, Models.Client client2)
    {
        try
        {
            Models.Client? createdClient = await _clientRepository.Create(client1);

            Assert.NotNull(createdClient);
            Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", createdClient.Id))).FirstOrDefault<Models.Client?>();
            Assert.NotNull(retrievedClient);
            AssertFieldsExpectedValues(client1, retrievedClient, new() { { "_id", retrievedClient.Id }, { Models.Client.UPDATED_AT, retrievedClient.UpdatedAt }, { Models.Client.CREATED_AT, retrievedClient.CreatedAt } });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client1.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client1.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client2);

        client1.Secret = client2.Secret;
        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _clientRepository.Create(client1));
        }
        finally
        {
            await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client2.Id));
            await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client1.Id));
        }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client1.Id))).FirstOrDefault<Models.Client?>());
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client2.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveById(Models.Client client)
    {
        Models.Client? retrievedClient = null;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            retrievedClient = await _clientRepository.RetrieveById(client.Id);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        retrievedClient = await _clientRepository.RetrieveById(client.Id);

        Assert.Null(retrievedClient);

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveBySecret(Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveBySecret(client.Secret);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveBySecret("client.Secret");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveByIdAndSecret(Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndSecret(client.Id, client.Secret);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndSecret(client.Id, "client.Secret");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void RetrieveByIdAndRedirectUrl(Models.Client client)
    {
        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndRedirectUrl(client.Id, client.RedirectUrl);

            Assert.NotNull(retrievedClient);
            Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
            AssertFieldsExpectedValues(client, retrievedClient, new() { });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);

        try
        {
            Models.Client? retrievedClient = await _clientRepository.RetrieveByIdAndRedirectUrl(client.Id, "client.RedirectUrl");

            Assert.Null(retrievedClient);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(TwoClients))]
    public async void UpdateRedirectUrl(Models.Client client, Models.Client client2)
    {
        string newRedirectUrl = Faker.Internet.Url();

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            bool? result = await _clientRepository.UpdateRedirectUrl(newRedirectUrl, client.Id, client.Secret);

            Assert.True(result);
            Models.Client retrievedClient = (await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).First();
            AssertFieldsExpectedValues(client, retrievedClient, new() { { Models.Client.REDIRECT_URL, newRedirectUrl }, { Models.Client.UPDATED_AT, retrievedClient.UpdatedAt } });
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        await _clientCollection.InsertOneAsync(client);
        await _clientCollection.InsertOneAsync(client2);

        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _clientRepository.UpdateRedirectUrl(client2.RedirectUrl, client.Id, client.Secret));
        }
        finally { await _clientCollection.DeleteManyAsync(Builders<Models.Client>.Filter.Empty); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client2.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void DeleteBySecret(Models.Client client)
    {
        bool? result;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            result = await _clientRepository.DeleteBySecret(client.Secret);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        result = await _clientRepository.DeleteBySecret(client.Secret);
        Assert.False(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void DeleteById(Models.Client client)
    {
        bool? result;

        // Success
        await _clientCollection.InsertOneAsync(client);

        try
        {
            result = await _clientRepository.DeleteById(client.Id);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        result = await _clientRepository.DeleteById(client.Id);
        Assert.False(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void ClientExposed(Models.Client client)
    {
        bool? result;
        string newHashedSecret = "newHashedSecret";

        // Success
        await _clientCollection.InsertOneAsync(client);

        Models.Client oldClient = (await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).First();

        try
        {
            result = await _clientRepository.ClientExposed(client, newHashedSecret);

            Assert.True(result);
            Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", oldClient.Id))).FirstOrDefault<Models.Client?>());
            Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq(Models.Client.SECRET, newHashedSecret))).FirstOrDefault<Models.Client?>();
            Assert.NotNull(retrievedClient);
            Assert.NotNull(retrievedClient.TokensExposedAt);
            Assert.NotEqual(oldClient.Id.ToString(), retrievedClient.Id.ToString());
            Assert.Equal(oldClient.RedirectUrl, retrievedClient.RedirectUrl);
            Assert.Equal(newHashedSecret, retrievedClient.Secret);
            Assert.Equal<int>(oldClient.ExposedCount + 1, retrievedClient.ExposedCount);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        result = await _clientRepository.ClientExposed(client, newHashedSecret);
        Assert.Null(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
    }

    [Theory]
    [MemberData(nameof(OneClient))]
    public async void ClientExposed_byId(Models.Client client)
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
            Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
            Models.Client? retrievedClient = (await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq(Models.Client.SECRET, newHashedSecret))).FirstOrDefault<Models.Client?>();
            Assert.NotNull(retrievedClient);
            Assert.NotNull(retrievedClient.TokensExposedAt);
            Assert.NotEqual(client.Id.ToString(), retrievedClient.Id.ToString());
            Assert.Equal(client.RedirectUrl, retrievedClient.RedirectUrl);
            Assert.Equal(newHashedSecret, retrievedClient.Secret);
            Assert.Equal<int>(client.ExposedCount + 1, retrievedClient.ExposedCount);
        }
        finally { await _clientCollection.DeleteOneAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id)); }

        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());

        // Failure
        result = await _clientRepository.ClientExposed(client.Id, hashedSecret, newHashedSecret);
        Assert.Null(result);
        Assert.Null((await _clientCollection.FindAsync(Builders<Models.Client>.Filter.Eq("_id", client.Id))).FirstOrDefault<Models.Client?>());
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