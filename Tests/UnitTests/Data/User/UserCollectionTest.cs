using System.Runtime.Serialization;
using Bogus;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Data;
using user_management.Data.User;
using Xunit;

namespace user_management.Tests.UnitTests.Data.User;

public class UserCollectionTest
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<Models.User> _userCollection;
    private readonly UserRepository _userRepository;
    public static Faker Faker = new("en");

    public UserCollectionTest()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { EnvironmentName = "Development" });

        builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
        MongoContext mongoContext = new();
        builder.Configuration.GetSection("MongoDB").Bind(mongoContext);

        _mongoClient = MongoContext.GetMongoClient(mongoContext);
        _userCollection = _mongoClient.GetDatabase(mongoContext.DatabaseName).GetCollection<Models.User>(mongoContext.Collections.Users);

        _userRepository = new UserRepository(Options.Create<MongoContext>(mongoContext));

        MongoContext.Initialize(Options.Create<MongoContext>(mongoContext)).Wait();
    }

    private static Models.User TemplateUser() => new Models.User()
    {
        Id = ObjectId.GenerateNewId(),
        Privileges = new Models.Privilege[] { },
        UserPrivileges = new()
        {
            Readers = new Models.Reader[] { },
            AllReaders = new() { },
            Updaters = new Models.Updater[] { },
            AllUpdaters = new() { },
            Deleters = new Models.Deleter[] { },
        },
        Clients = new Models.UserClient[] {
            new() {
                RefreshToken = new() {
                    Code = (new Faker()).Random.AlphaNumeric(6),
                    Value = (new Faker()).Random.AlphaNumeric(6),
                },
                Token = new() {
                    Value = (new Faker()).Random.AlphaNumeric(6),
                }
            }
        },
        FirstName = (new Faker()).Person.FirstName,
        MiddleName = (new Faker()).Person.FirstName,
        LastName = (new Faker()).Person.LastName,
        Username = (new Faker()).Person.UserName,
        Email = (new Faker()).Person.Email,
        PhoneNumber = (new Faker()).Person.Phone,
        Password = (new Faker()).Internet.Password(),
        UpdatedAt = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };

    [Fact]
    public async void UserNameIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.Username = user1.Username;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void EmailIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.Email = user1.Email;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void PhoneNumberIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.PhoneNumber = user1.PhoneNumber;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void FullNameIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.FirstName = user1.FirstName;
        user2.MiddleName = user1.MiddleName;
        user2.LastName = user1.LastName;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void RefreshTokenCodeIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.Clients[0].RefreshToken!.Code = user1.Clients[0].RefreshToken!.Code;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void RefreshTokenValueIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.Clients[0].RefreshToken!.Value = user1.Clients[0].RefreshToken!.Value;
        await TestIndex(user1, user2);
    }

    [Fact]
    public async void TokenValueIndex()
    {
        Models.User user1 = TemplateUser(), user2 = TemplateUser();
        user2.Clients[0].Token!.Value = user1.Clients[0].Token!.Value;
        await TestIndex(user1, user2);
    }

    private async Task TestIndex(Models.User user1, Models.User user2)
    {
        IClientSessionHandle? session = null;
        try
        {
            session = await _mongoClient.StartSessionAsync();

            session.StartTransaction(new(writeConcern: WriteConcern.WMajority));

            await _userCollection.InsertOneAsync(session, user1);

            await Assert.ThrowsAsync<MongoWriteException>(async () => await _userCollection.InsertOneAsync(session, user2));

            await session.AbortTransactionAsync();
        }
        catch (Exception) { if (session != null) await session.AbortTransactionAsync(); throw; }
        finally { if (session != null) session.Dispose(); }
    }
}