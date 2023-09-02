using System.Reflection;
using Bogus;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using user_management.Data;
using user_management.Data.User;
using user_management.Services.Data;
using Xunit;

namespace user_management.Tests.IntegrationTests.Data.User;

public class UserRepositoryTest
{
    private readonly MongoClient _mongoClient;
    private readonly IMongoCollection<Models.User> _userCollection;
    private readonly IMongoDatabase _mongoDatabase;
    private readonly UserRepository _userRepository;
    public static Faker Faker = new("en");

    public UserRepositoryTest()
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions() { EnvironmentName = "Development" });

        builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
        MongoContext mongoContext = new();
        builder.Configuration.GetSection("MongoDB").Bind(mongoContext);

        _mongoClient = MongoContext.GetMongoClient(mongoContext);
        _mongoDatabase = _mongoClient.GetDatabase(mongoContext.DatabaseName);
        _userCollection = _mongoDatabase.GetCollection<Models.User>(mongoContext.Collections.Users);

        _userRepository = new UserRepository(Options.Create<MongoContext>(mongoContext));

        MongoContext.Initialize(Options.Create<MongoContext>(mongoContext)).Wait();
    }

    private static Models.User TemplateUser() => new Models.User()
    {
        Id = ObjectId.GenerateNewId(),
        Privileges = StaticData.GetDefaultUserPrivileges().ToArray(),
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
                ClientId = ObjectId.GenerateNewId(),
                RefreshToken = new() {
                    Code = ObjectId.GenerateNewId().ToString(),
                    Value = ObjectId.GenerateNewId().ToString(),
                },
                Token = new() {
                    Value = ObjectId.GenerateNewId().ToString(),
                }
            },
            new() {
                ClientId = ObjectId.GenerateNewId(),
                RefreshToken = new() {
                    Code = ObjectId.GenerateNewId().ToString(),
                    Value = ObjectId.GenerateNewId().ToString(),
                },
                Token = new() {
                    Value = ObjectId.GenerateNewId().ToString(),
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
        IsVerified = (new Faker()).Random.Bool(),
        LoggedOutAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow),
        VerificationSecret = (new Faker()).Random.String2(length: 6),
        VerificationSecretUpdatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
        UpdatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow),
        CreatedAt = (new Faker()).Date.Between(DateTime.UtcNow.AddDays(-14), DateTime.UtcNow.AddDays(-8))
    };

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<Models.User> GenerateUsers(int count = 1)
    {
        IEnumerable<Models.User> users = new Models.User[] { };
        for (int i = 0; i < count; i++)
        {
            Models.User user = TemplateUser();
            int safety = 0;
            do { user = TemplateUser(); safety++; }
            while (safety < 500 && users.FirstOrDefault<Models.User?>(u => u != null && (
                u.Username == user.Username ||
                u.Email == user.Email ||
                u.PhoneNumber == user.PhoneNumber ||
                (
                    u.FirstName == user.FirstName &&
                    u.MiddleName == user.MiddleName &&
                    u.LastName == user.LastName
                )
            )) != null);
            if (safety >= 500) throw new Exception("While loop safety triggered at GenerateUsers private method of UserRepositoryTests.");

            users = users.Append(user);
        }

        return users;
    }

    public static IEnumerable<object?[]> OneUser =>
        new List<object?[]>
        {
            new object?[] {
                GenerateUsers().ElementAt(0)
            },
        };

    public static IEnumerable<object?[]> TwoUsers =>
        new List<object?[]>
        {
            GenerateUsers(2).ToArray(),
        };

    public static IEnumerable<object?[]> ManyUsers =>
        new List<object?[]>
        {
            new object?[] {
                GenerateUsers(20).ToArray()
            },
        };

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Create(Models.User user1, Models.User user2)
    {
        try
        {
            Models.User? createdUser = await _userRepository.Create(user1);

            Assert.NotNull(createdUser);
            Assert.NotNull((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", createdUser.Id))).FirstOrDefault<Models.User?>());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());

        await _userCollection.InsertOneAsync(user2);

        user1.Username = user2.Username;
        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _userRepository.Create(user1));
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user2.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user2.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByFullNameForExistenceCheck(Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            Models.User? user = await _userRepository.RetrieveByFullNameForExistenceCheck(user1.FirstName!, user1.MiddleName!, user1.LastName!);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByUsernameForExistenceCheck(Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            Models.User? user = await _userRepository.RetrieveByUsernameForExistenceCheck(user1.Username);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByEmailForExistenceCheck(Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            Models.User? user = await _userRepository.RetrieveByEmailForExistenceCheck(user1.Email);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByPhoneNumberForExistenceCheck(Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            Models.User? user = await _userRepository.RetrieveByPhoneNumberForExistenceCheck(user1.PhoneNumber!);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveById_WithoutPrivilegeCheck(Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            Models.User? user = await _userRepository.RetrieveById((ObjectId)user1.Id);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user1.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void RetrieveById(Models.User readerUser, Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(readerUser);

        var readers = user.UserPrivileges.Readers.ToList();
        Models.Field[] targetFieldsToRead = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
        readers.Add(new Models.Reader() { Author = Models.Reader.USER, AuthorId = (ObjectId)readerUser.Id, IsPermitted = true, Fields = targetFieldsToRead });
        user.UserPrivileges.Readers = readers.ToArray();
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.PartialUser? retrievedUser = await _userRepository.RetrieveById((ObjectId)readerUser.Id, (ObjectId)user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());

            PropertyInfo[] retrievedUserProperties = retrievedUser.GetType().GetProperties();
            retrievedUserProperties.ToList().ForEach(p =>
            {
                // Find db related properties
                CustomAttributeData? customAttribute = p.CustomAttributes.FirstOrDefault(ca => ca != null && (ca.AttributeType == typeof(BsonElementAttribute) || ca.AttributeType == typeof(BsonIdAttribute)));
                if (customAttribute == null) return;

                if (customAttribute.AttributeType == typeof(BsonIdAttribute))
                {
                    Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
                    return;
                }

                string fieldName = (customAttribute.ConstructorArguments[0].Value as string)!;

                // If this property exists in chosen fields to read (targetFieldsToRead)
                if (targetFieldsToRead.FirstOrDefault<Models.Field?>(f => f != null && f.Name == fieldName) != null)
                {
                    // it should be touched
                    var method = retrievedUser.GetType().GetMethod("Is" + p.Name + "Touched");
                    Assert.NotNull(method);
                    Assert.True((bool)method.Invoke(retrievedUser, new object?[] { })!);
                }
                else
                {
                    // Otherwise it should be Untouched
                    var method = retrievedUser.GetType().GetMethod("Is" + p.Name + "Touched");
                    Assert.NotNull(method);
                    Assert.False((bool)method.Invoke(retrievedUser, new object?[] { })!);
                }

            });
        }
        finally
        {
            // Delete users
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", readerUser.Id));
        }

        // Assert user deletion
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", readerUser.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_sorting(Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // Sort users by their created_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].CreatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPrivileges.Readers.ToList();

            Models.Field[] targetFieldsToRead = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == Models.User.CREATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Models.Field() { IsPermitted = true, Name = Models.User.CREATED_AT }).ToArray();

            readers.Add(new Models.Reader()
            {
                Author = Models.Reader.USER,
                AuthorId = (ObjectId)readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPrivileges.Readers = readers.ToArray();
        }

        // Persist users to the db
        for (int i = 0; i < users.Count(); i++)
            await _userCollection.InsertOneAsync(users.ElementAt(i));

        // The actual test
        try
        {
            DateTime? previousDt = null;
            for (int currentIteration = 0; currentIteration < maxIteration; currentIteration++)
            {
                List<Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, "empty", limit, currentIteration, Models.User.CREATED_AT);

                retrievedUsers.ForEach(retrievedUser =>
                {
                    if (previousDt != null) Assert.True(retrievedUser.CreatedAt > previousDt);
                    else previousDt = retrievedUser.CreatedAt;
                });
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_pagination(Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // sort users by their update_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].UpdatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPrivileges.Readers.ToList();

            Models.Field[] targetFieldsToRead = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == Models.User.UPDATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Models.Field() { IsPermitted = true, Name = Models.User.UPDATED_AT }).ToArray();

            readers.Add(new Models.Reader()
            {
                Author = Models.Reader.USER,
                AuthorId = (ObjectId)readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPrivileges.Readers = readers.ToArray();
        }

        // Persist users to the db
        for (int i = 0; i < users.Count(); i++)
            await _userCollection.InsertOneAsync(users.ElementAt(i));

        // The actual test
        try
        {
            for (int currentIteration = 0; currentIteration < maxIteration; currentIteration++)
            {
                int expectedUserCount = (currentIteration == (maxIteration - 1) && users.Length % limit != 0) ? users.Length % limit : limit;

                List<Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, "empty", limit, currentIteration, null);

                Assert.Equal<int>(expectedUserCount, retrievedUsers.Count());

                for (int ru = 0; ru < retrievedUsers.Count(); ru++)
                    Assert.True(retrievedUsers[ru].Id.ToString() == users[(currentIteration * limit) + ru].Id.ToString());
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_logicsString(Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // Sort users by their created_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].CreatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPrivileges.Readers.ToList();

            Models.Field[] targetFieldsToRead = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == Models.User.CREATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Models.Field() { IsPermitted = true, Name = Models.User.CREATED_AT }).ToArray();

            readers.Add(new Models.Reader()
            {
                Author = Models.Reader.USER,
                AuthorId = (ObjectId)readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPrivileges.Readers = readers.ToArray();
        }

        // Persist users to the db
        for (int i = 0; i < users.Count(); i++)
            await _userCollection.InsertOneAsync(users.ElementAt(i));

        // The actual test
        try
        {
            for (int currentIteration = 0; currentIteration < maxIteration; currentIteration++)
            {
                List<Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, "empty", limit, currentIteration, Models.User.CREATED_AT);

                Assert.NotEmpty(retrievedUsers);

                retrievedUsers.ForEach(retrievedUser =>
                {
                    PropertyInfo[] retrievedUserProperties = retrievedUser.GetType().GetProperties();
                    retrievedUserProperties.ToList().ForEach(p =>
                    {
                        // Find db related properties
                        CustomAttributeData? customAttribute = p.CustomAttributes.FirstOrDefault(ca => ca != null && (ca.AttributeType == typeof(BsonElementAttribute) || ca.AttributeType == typeof(BsonIdAttribute)));
                        if (customAttribute == null) return;

                        if (customAttribute.AttributeType == typeof(BsonIdAttribute)) return;

                        string fieldName = (customAttribute.ConstructorArguments[0].Value as string)!;

                        Models.Field[] targetFieldsToRead = users.First(u => u.Id.ToString() == retrievedUser.Id.ToString()).UserPrivileges!.Readers[0].Fields;
                        // If this property exists in chosen fields to read (targetFieldsToRead)
                        if (targetFieldsToRead.FirstOrDefault<Models.Field?>(f => f != null && f.Name == fieldName) != null)
                        {
                            // it should be touched
                            var method = retrievedUser.GetType().GetMethod("Is" + p.Name + "Touched");
                            Assert.NotNull(method);
                            Assert.True((bool)method.Invoke(retrievedUser, new object?[] { })!);
                        }
                        else
                        {
                            // Otherwise it should be Untouched
                            var method = retrievedUser.GetType().GetMethod("Is" + p.Name + "Touched");
                            Assert.NotNull(method);
                            Assert.False((bool)method.Invoke(retrievedUser, new object?[] { })!);
                        }
                    });
                });
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByIdForAuthenticationHandling(Models.User user)
    {
        // Success
        user.IsVerified = true;
        user.LoggedOutAt = null;

        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Null(user.LoggedOutAt);
            Assert.True(user.IsVerified);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());

        // Failure
        user.IsVerified = true;
        user.LoggedOutAt = DateTime.UtcNow.AddMinutes(-7);

        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.Null(retrievedUser);
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());

        user.IsVerified = false;
        user.LoggedOutAt = null;

        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.Null(retrievedUser);
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByIdForAuthorizationHandling(Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthorizationHandling(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveByIdForAuthorizationHandling(user.Id));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForPasswordChange(Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveUserForPasswordChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForPasswordChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForUsernameChange(Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveUserForUsernameChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForUsernameChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForEmailChange(Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveUserForEmailChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForEmailChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForPhoneNumberChange(Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveUserForPhoneNumberChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForPhoneNumberChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByClientIdAndCode(Models.User user)
    {
        string code = user.Clients[0].RefreshToken!.Code!;
        ObjectId clientId = user.Clients[0].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByClientIdAndCode(clientId, code);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.Clients[0].ClientId, retrievedUser.Clients[0].ClientId);
            Assert.Equal(user.Clients[0].RefreshToken!.Code!, retrievedUser.Clients[0].RefreshToken!.Code!);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First(), new() { { Models.User.LOGGED_OUT_AT, null } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveByClientIdAndCode(clientId, code));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByRefreshTokenValue(Models.User user)
    {
        string token = user.Clients[0].RefreshToken!.Value;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByRefreshTokenValue(token);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.Clients[0].RefreshToken!.Value, retrievedUser.Clients[0].RefreshToken!.Value);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveByRefreshTokenValue(token));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByTokenValue(Models.User user)
    {
        string token = user.Clients[0].Token!.Value!;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            Models.User? retrievedUser = await _userRepository.RetrieveByTokenValue(token);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.Clients[0].Token!.Value!, retrievedUser.Clients[0].Token!.Value!);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null(await _userRepository.RetrieveByTokenValue(token));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Login(Models.User user)
    {
        string token = user.Clients[0].Token!.Value!;

        // Success
        Assert.NotNull(user.LoggedOutAt);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Login(user.Id);

            Assert.True(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First(), new() { { Models.User.LOGGED_OUT_AT, null } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecret(Models.User user)
    {
        string VerificationSecret = "VerificationSecret";

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecret(VerificationSecret, user.Email);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecretForActivation(Models.User user)
    {
        user.IsVerified = false;
        string VerificationSecret = "VerificationSecret";

        // Success
        Assert.False(user.IsVerified);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForActivation(VerificationSecret, user.Email);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());

        // Failure
        user.IsVerified = true;

        Assert.True(user.IsVerified);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForActivation(VerificationSecret, user.Email);

            Assert.Null(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First(), new() { });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecretForPasswordChange(Models.User user)
    {
        string VerificationSecret = "VerificationSecret";

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForPasswordChange(VerificationSecret, user.Email);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Verify(Models.User user)
    {
        user.IsVerified = false;
        // Success
        Assert.False(user.IsVerified);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Verify(user.Id);

            Assert.True(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First(), new() { { Models.User.IS_VERIFIED, true } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangePassword(Models.User user)
    {
        string hashedPassword = "hashedPassword";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangePassword(user.Email, hashedPassword);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.PASSWORD, hashedPassword }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangeUsername(Models.User user)
    {
        string username = "username";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangeUsername(user.Email, username);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USERNAME, username }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangePhoneNumber(Models.User user)
    {
        string phoneNumber = "phoneNumber";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangePhoneNumber(user.Email, phoneNumber);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.PHONE_NUMBER, phoneNumber }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangeEmail(Models.User user)
    {
        string newEmail = "newEmail";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangeEmail(user.Email, newEmail);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.EMAIL, newEmail }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Logout(Models.User user)
    {
        user.LoggedOutAt = null;

        // Success
        Assert.Null(user.LoggedOutAt);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Logout(user.Id);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.LOGGED_OUT_AT, user.LoggedOutAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void RemoveClient(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] {
            new() {
                AuthorId = actor.Id,
                Author = Models.Reader.USER,
                IsPermitted = true,
                Fields = new Models.Field[] { new() { Name = Models.User.CLIENTS, IsPermitted = true } }
            }
        };

        user.UserPrivileges.Updaters = new Models.Updater[] {
            new() {
                AuthorId = actor.Id,
                Author = Models.Updater.USER,
                IsPermitted = true,
                Fields = new Models.Field[] { new() { Name = Models.User.CLIENTS, IsPermitted = true } }
            }
        };

        // Success
        Assert.True(user.Clients.Length >= 1);
        Assert.True(user.UserPrivileges.Readers.FirstOrDefault(r => r != null && r.AuthorId == actor.Id && r.IsPermitted && r.Author == Models.Reader.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == Models.User.CLIENTS && f.IsPermitted) != null) != null);
        Assert.True(user.UserPrivileges.Updaters.FirstOrDefault(r => r != null && r.AuthorId == actor.Id && r.IsPermitted && r.Author == Models.Updater.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == Models.User.CLIENTS && f.IsPermitted) != null) != null);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.RemoveClient(user.Id, user.Clients[0].ClientId, actor.Id, false);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.CLIENTS, user.Clients.Where(uc => uc.ClientId != user.Clients[0].ClientId).ToArray() }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RemoveAllClients(Models.User user)
    {
        user.UserPrivileges.Readers = new Models.Reader[] {
            new() {
                AuthorId = user.Id,
                Author = Models.Reader.USER,
                IsPermitted = true,
                Fields = new Models.Field[] { new() { Name = Models.User.CLIENTS, IsPermitted = true } }
            }
        };

        user.UserPrivileges.Updaters = new Models.Updater[] {
            new() {
                AuthorId = user.Id,
                Author = Models.Updater.USER,
                IsPermitted = true,
                Fields = new Models.Field[] { new() { Name = Models.User.CLIENTS, IsPermitted = true } }
            }
        };

        // Success
        Assert.True(user.Clients.Length >= 0);
        Assert.True(user.UserPrivileges.Readers.FirstOrDefault(r => r != null && r.AuthorId == user.Id && r.IsPermitted && r.Author == Models.Reader.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == Models.User.CLIENTS && f.IsPermitted) != null) != null);
        Assert.True(user.UserPrivileges.Updaters.FirstOrDefault(r => r != null && r.AuthorId == user.Id && r.IsPermitted && r.Author == Models.Updater.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == Models.User.CLIENTS && f.IsPermitted) != null) != null);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.RemoveAllClients(user.Id, user.Id, false);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.CLIENTS, new Models.UserClient[] { } }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void VerifyRefreshToken(Models.User user)
    {
        user.Clients[0].RefreshToken!.IsVerified = false;
        Assert.False(user.Clients[0].RefreshToken!.IsVerified);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.VerifyRefreshToken(user.Id, user.Clients[0].ClientId, null);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            Assert.True(retrievedUser.Clients[0].RefreshToken!.IsVerified);
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.CLIENTS, user.Clients }, { Models.User.UPDATED_AT, user.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddToken(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.CLIENTS } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.CLIENTS } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };
        string tokenValue = Faker.Random.String2(40);
        DateTime expirationDate = Faker.Date.Between(DateTime.UtcNow, DateTime.UtcNow.AddDays(3));
        user.Clients[0].Token = null;

        // Success
        Assert.True(user.Clients.Length >= 1);
        await _userCollection.InsertOneAsync(user);

        user.Clients[0].Token = new() { ExpirationDate = expirationDate, IsRevoked = false, Value = tokenValue };

        Models.UserClient[] newCLientsObject = user.Clients;
        try
        {
            bool? result = await _userRepository.AddToken(user.Id, actor.Id, user.Clients[0].ClientId, user.Clients[0].Token!);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.CLIENTS, user.Clients }, { Models.User.UPDATED_AT, user.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_readsFields(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };
        Models.TokenPrivileges tokenPrivileges = new() { ReadsFields = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };
        ObjectId clientId = user.Clients[1].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPrivileges.Readers = user.UserPrivileges.Readers.Append(new() { Author = Models.Reader.CLIENT, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_updatesFields(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Updater.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };
        Models.TokenPrivileges tokenPrivileges = new() { UpdatesFields = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };
        ObjectId clientId = user.Clients[1].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPrivileges.Updaters = user.UserPrivileges.Updaters.Append(new() { Author = Models.Updater.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_deletes(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };
        Models.TokenPrivileges tokenPrivileges = new() { DeletesUser = true };
        ObjectId clientId = user.Clients[1].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPrivileges.Deleters = user.UserPrivileges.Deleters.Append(new() { Author = Models.Deleter.USER, AuthorId = clientId, IsPermitted = true }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            Assert.True(retrievedUser.UserPrivileges.Deleters[0].AuthorId == clientId && retrievedUser.UserPrivileges.Deleters[0].IsPermitted);
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_userAlreadyPrivileged(Models.User user, Models.User actor)
    {
        ObjectId clientId = user.Clients[1].ClientId;
        Models.TokenPrivileges tokenPrivileges = new() { ReadsFields = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };

        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = new Models.Field[] { } }, new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields } };

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_allCases(Models.User user, Models.User actor)
    {
        ObjectId clientId = user.Clients[1].ClientId;
        Models.TokenPrivileges tokenPrivileges = new()
        {
            ReadsFields = Faker.PickRandom<Models.Field>(Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray(),
            UpdatesFields = Faker.PickRandom<Models.Field>(Models.User.GetMassUpdatableFields(), (int)(Faker.Random.Int(1, 5))).ToArray(),
            DeletesUser = true,
        };

        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = new Models.Field[] { } }, new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllReaders = new() { };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.AllUpdaters = new() { };
        user.UserPrivileges.Deleters = new Models.Deleter[] { };

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields } };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Updater.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields } };
        user.UserPrivileges.Deleters = new Models.Deleter[] { new() { Author = Models.Deleter.USER, AuthorId = clientId, IsPermitted = true } };

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddClientById(Models.User user, Models.User actor)
    {
        Models.UserClient userClient = new()
        {
            ClientId = ObjectId.GenerateNewId(),
            RefreshToken = new()
            {
                TokenPrivileges = new() { DeletesUser = true },
                Code = Faker.Random.String2(128),
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(2),
                CodeChallenge = Faker.Random.String2(40),
                CodeChallengeMethod = "SHA215",
                ExpirationDate = DateTime.UtcNow.AddMonths(3),
                Value = Faker.Random.String2(128),
                IsVerified = false
            },
            Token = null
        };
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.CLIENTS } } } };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.CLIENTS } } } };

        // Success
        await _userCollection.InsertOneAsync(user);

        user.Clients = user.Clients.Append(userClient).ToArray();

        try
        {
            bool? result = await _userRepository.AddClientById(user.Id, actor.Id, userClient);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.CLIENTS, user.Clients }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void UpdateUserPrivileges(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };
        user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Updater.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.USER_PRIVILEGES } } } };

        // Success
        await _userCollection.InsertOneAsync(user);

        user.UserPrivileges.Updaters = new Models.Updater[] { new() {
            Author = Models.Updater.USER, AuthorId = ObjectId.GenerateNewId(),
            IsPermitted = true,
            Fields = Faker.PickRandom<Models.Field>(Models.User.GetUpdatableFields(), (int)Faker.Random.Int(1,3)).ToArray()
        }};

        try
        {
            bool? result = await _userRepository.UpdateUserPrivileges(actor.Id, user.Id, user.UserPrivileges);

            Assert.True(result);
            Models.User retrievedUser = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { Models.User.USER_PRIVILEGES, user.UserPrivileges }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id)); }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Update(Models.User[] users)
    {
        Models.User actor = users[0];
        users = users.Where((u, i) => i != 0).ToArray();

        actor.UserPrivileges.Readers = new Models.Reader[] { };
        for (int i = 0; i < users.Length; i++)
        {
            Models.User user = users[i];
            user.UserPrivileges.Readers = new Models.Reader[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.FIRST_NAME } } } };
            user.UserPrivileges.Updaters = new Models.Updater[] { new() { Author = Models.Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Models.Field[] { new() { IsPermitted = true, Name = Models.User.FIRST_NAME } } } };
        }

        // Success
        await _userCollection.InsertOneAsync(actor);
        for (int i = 0; i < users.Length; i++)
            await _userCollection.InsertOneAsync(users[i]);

        try
        {
            bool? result = await _userRepository.Update(actor.Id, "empty", "first_name::Set::test_first_name::string");

            Assert.True(result);
            List<Models.User> retrievedUsers = (await _userCollection.FindAsync(Builders<Models.User>.Filter.Ne("_id", actor.Id))).ToList();
            for (int i = 0; i < retrievedUsers.Count; i++)
            {
                Models.User retrievedUser = retrievedUsers[i];
                Assert.Equal("test_first_name", retrievedUser.FirstName);
                AssertFieldsExpectedValues(users[i], retrievedUser, new() { { Models.User.FIRST_NAME, "test_first_name" }, { Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<Models.User>.Filter.Empty); }

        Assert.Empty((await _userCollection.FindAsync(Builders<Models.User>.Filter.Ne("_id", actor.Id))).ToList());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Delete_success(Models.User user, Models.User actor)
    {
        user.UserPrivileges.Deleters = user.UserPrivileges.Deleters.Append(new() { Author = Models.Deleter.USER, AuthorId = actor.Id, IsPermitted = true }).ToArray();
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Delete(actor.Id, user.Id);

            Assert.True(result);
            Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", user.Id));
            await _userCollection.DeleteOneAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Delete_failure(Models.User user, Models.User actor)
    {
        // Failure
        bool? result = await _userRepository.Delete(actor.Id, user.Id);

        Assert.Null(result);

        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", user.Id))).FirstOrDefault<Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<Models.User>.Filter.Eq("_id", actor.Id))).FirstOrDefault<Models.User?>());
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
