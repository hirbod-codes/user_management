using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using user_management.Data;

namespace user_management_integration_tests.Data.User;

[CollectionDefinition("UserCollectionTest", DisableParallelization = true)]
public class UserCollectionTestCollectionDefinition { }

[Collection("UserCollectionTest")]
public class UserCollectionTest : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoCollection<user_management.Models.User> _userCollection;
    public static Faker Faker = new("en");

    public UserCollectionTest(CustomWebApplicationFactory<Program> factory)
    {
        MongoCollections mongoCollections = factory.Services.GetService<MongoCollections>()!;
        IMongoDatabase mongoDatabase = factory.Services.GetService<IMongoDatabase>()!;
        mongoCollections.ClearCollections(mongoDatabase).Wait();

        _mongoClient = factory.Services.GetService<IMongoClient>()!;

        _userCollection = mongoCollections.Users;
    }

    private static user_management.Models.User TemplateUser(IEnumerable<user_management.Models.User>? users = null)
    {
        IEnumerable<user_management.Models.Client>? clients = new user_management.Models.Client[] { };
        for (int i = 0; i < 5; i++)
            clients = clients.Append(user_management.Models.Client.FakeClient(out string secret, clients)).ToArray();

        user_management.Models.User user = user_management.Models.User.FakeUser(clients: clients);

        return user;
    }

    [Fact]
    public async void UserNameIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();
        user2.Username = user1.Username;
        user3.Username = user1.Username;
        await TestUniqueIndex(user1, user2, user3);
    }

    [Fact]
    public async void EmailIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();
        user2.Email = user1.Email;
        user3.Email = user1.Email;
        await TestUniqueIndex(user1, user2, user3);
    }

    [Fact]
    public async void PhoneNumberIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();
        if (user1.PhoneNumber == null) user1.PhoneNumber = Faker.Phone.PhoneNumber();

        user2.PhoneNumber = user1.PhoneNumber;
        user3.PhoneNumber = user1.PhoneNumber;
        await TestUniqueIndex(user1, user2, user3);

        user1.PhoneNumber = null;
        user2.PhoneNumber = null;
        await TestNullability(user1, user2, user3);
    }

    [Fact]
    public async void FullNameIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();

        if (user1.FirstName == null || user1.MiddleName == null || user1.LastName == null)
        {
            user1.FirstName = Faker.Person.FirstName;
            user1.MiddleName = Faker.Person.FirstName;
            user1.LastName = Faker.Person.LastName;
        }

        user2.FirstName = user1.FirstName;
        user2.MiddleName = user1.MiddleName;
        user2.LastName = user1.LastName;
        user3.FirstName = user1.FirstName;
        user3.MiddleName = user1.MiddleName;
        user3.LastName = user1.LastName;
        await TestUniqueIndex(user1, user2, user3);

        user1.FirstName = null;
        user1.MiddleName = null;
        user1.LastName = null;
        user2.FirstName = null;
        user2.MiddleName = null;
        user2.LastName = null;
        user3.FirstName = null;
        user3.MiddleName = null;
        user3.LastName = null;
        await TestNullability(user1, user2, user3);
    }

    [Fact]
    public async void RefreshTokenCodeIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();
        user1.AuthorizingClient = new() { Code = Faker.Random.String2(168) };
        user2.AuthorizingClient = new() { Code = Faker.Random.String2(168) };
        user3.AuthorizingClient = new() { Code = Faker.Random.String2(168) };
        user2.AuthorizingClient.Code = user1.AuthorizingClient.Code;
        user3.AuthorizingClient.Code = user1.AuthorizingClient.Code;
        await TestUniqueIndex(user1, user2, user3);

        user1.AuthorizingClient = null;
        user2.AuthorizingClient = null;
        user3.AuthorizingClient = null;
        await TestNullability(user1, user2, user3);
    }

    [Fact]
    public async void RefreshTokenValueIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();

        if (user1.Clients.Count() == 0 || user2.Clients.Count() == 0 || user3.Clients.Count() == 0)
        {
            user1.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret1)) };
            user2.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret2)) };
            user3.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret3)) };
        }

        user2.Clients[0].RefreshToken!.Value = user1.Clients[0].RefreshToken!.Value;
        user3.Clients[0].RefreshToken!.Value = user1.Clients[0].RefreshToken!.Value;
        await TestUniqueIndex(user1, user2, user3);
    }

    [Fact]
    public async void TokenValueIndex()
    {
        user_management.Models.User user1 = TemplateUser(), user2 = TemplateUser(), user3 = TemplateUser();

        if (user1.Clients.Count() == 0 || user2.Clients.Count() == 0 || user3.Clients.Count() == 0)
        {
            user1.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret1)) };
            user2.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret2)) };
            user3.Clients = new user_management.Models.UserClient[] { user_management.Models.UserClient.FakeUserClient(user_management.Models.Client.FakeClient(out string secret3)) };
        }

        user2.Clients[0].Token!.Value = user1.Clients[0].Token!.Value;
        user3.Clients[0].Token!.Value = user1.Clients[0].Token!.Value;
        await TestUniqueIndex(user1, user2, user3);
    }

    private async Task TestUniqueIndex(user_management.Models.User user1, user_management.Models.User user2, user_management.Models.User? user3 = null)
    {
        List<user_management.Models.User> users = new List<user_management.Models.User>() { user1, user2 };
        if (user3 != null) users.Add(user3);

        await Assert.ThrowsAsync<MongoDB.Driver.MongoBulkWriteException<user_management.Models.User>>(async () => await _userCollection.InsertManyAsync(users));

        await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty);
    }

    private async Task TestNullability(user_management.Models.User user1, user_management.Models.User user2, user_management.Models.User? user3 = null)
    {
        List<user_management.Models.User> users = new List<user_management.Models.User>() { user1, user2 };
        if (user3 != null) users.Add(user3);

        await _userCollection.InsertManyAsync(users);

        Assert.NotNull((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<user_management.Models.User?>());
        Assert.NotNull((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", user2.Id))).FirstOrDefault<user_management.Models.User?>());
        if (user3 != null) Assert.NotNull((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", user3.Id))).FirstOrDefault<user_management.Models.User?>());

        await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty);
    }
}
