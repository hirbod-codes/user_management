using System.Reflection;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using user_management.Data.Logics;
using user_management.Data.MongoDB;
using user_management.Data.MongoDB.User;
using user_management.Data.Seeders;
using user_management.Models;
using user_management.Services.Data;

namespace user_management_integration_tests.Data.User;

[CollectionDefinition("UserRepositoryTest", DisableParallelization = true)]
public class UserRepositoryTestCollectionDefinition { }

[Collection("UserRepositoryTest")]
public class UserRepositoryTest : IAsyncLifetime, IClassFixture<CustomWebApplicationFactory<Program>>
{
    public const string USERS_PASSWORDS = "Pass%w0rd!99";

    private readonly MongoCollections _mongoCollections;
    private readonly IMongoCollection<user_management.Models.User> _userCollection;
    private readonly IMongoCollection<user_management.Models.Client> _clientCollection;
    private readonly UserRepository _userRepository;
    private readonly IMongoDatabase _mongoDatabase;
    public static Faker Faker = new("en");
    private static IEnumerable<user_management.Models.Client> _clients = Array.Empty<user_management.Models.Client>();

    public UserRepositoryTest(CustomWebApplicationFactory<Program> factory)
    {
        _mongoCollections = factory.Services.GetService<MongoCollections>()!;
        _mongoDatabase = factory.Services.GetService<IMongoDatabase>()!;
        _userCollection = _mongoCollections.Users;
        _clientCollection = _mongoCollections.Clients;

        _userRepository = new(_mongoCollections, factory.Services.GetService<MongoContext>()!.GetClient());
    }

    public Task InitializeAsync() => _mongoCollections.ClearCollections(_mongoDatabase);

    public Task DisposeAsync() => Task.CompletedTask;

    /// <exception cref="System.Exception"></exception>
    public static IEnumerable<user_management.Models.User> GenerateUsers(int count = 1)
    {
        if (_clients.Count() == 0)
            for (int i = 0; i < 5; i++)
                _clients = _clients.Append(new ClientSeeder().FakeClient(ObjectId.GenerateNewId().ToString(), out string secret, _clients));

        IEnumerable<user_management.Models.User> users = Array.Empty<user_management.Models.User>();

        for (int i = 0; i < count; i++)
        {
            user_management.Models.User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), users, _clients, password: USERS_PASSWORDS);

            if (user.AuthorizedClients.Length == 0) user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(_clients.ElementAt(0))).ToArray();

            users = users.Append(user);
        }

        return users;
    }

    public static IEnumerable<object?[]> OneUser => new List<object?[]> { new object?[] { GenerateUsers().ElementAt(0) } };
    public static IEnumerable<object?[]> TwoUsers => new List<object?[]> { GenerateUsers(2).ToArray() };
    public static IEnumerable<object?[]> ManyUsers => new List<object?[]> { new object?[] { GenerateUsers(20).ToArray() } };

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Create(user_management.Models.User user1, user_management.Models.User user2)
    {
        try
        {
            user_management.Models.User? createdUser = await _userRepository.Create(user1);

            Assert.NotNull(createdUser);
            Assert.NotNull((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(createdUser.Id)))).FirstOrDefault<user_management.Models.User?>());
        }
        finally { await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id))); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());

        await _userCollection.InsertOneAsync(user2);

        user1.Username = user2.Username;
        try
        {
            await Assert.ThrowsAsync<DuplicationException>(async () => await _userRepository.Create(user1));
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user2.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByFullNameForExistenceCheck(user_management.Models.User user1)
    {
        if (user1.FirstName == null && user1.MiddleName == null && user1.LastName == null) user1.FirstName = "a first name";

        await _userCollection.InsertOneAsync(user1);

        try
        {
            user_management.Models.User? user = await _userRepository.RetrieveByFullNameForExistenceCheck(user1.FirstName!, user1.MiddleName!, user1.LastName!);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());

        await Assert.ThrowsAsync<ArgumentException>(async () => await _userRepository.RetrieveByFullNameForExistenceCheck(null, null, null));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByUsernameForExistenceCheck(user_management.Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            user_management.Models.User? user = await _userRepository.RetrieveByUsernameForExistenceCheck(user1.Username);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByEmailForExistenceCheck(user_management.Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            user_management.Models.User? user = await _userRepository.RetrieveByEmailForExistenceCheck(user1.Email);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByPhoneNumberForExistenceCheck(user_management.Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            user_management.Models.User? user = await _userRepository.RetrieveByPhoneNumberForExistenceCheck(user1.PhoneNumber!);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveById_WithoutPrivilegeCheck(user_management.Models.User user1)
    {
        await _userCollection.InsertOneAsync(user1);

        try
        {
            user_management.Models.User? user = await _userRepository.RetrieveById(user1.Id);

            Assert.NotNull(user);
            Assert.Equal(user1.Id.ToString(), user.Id.ToString());
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user1.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void RetrieveById(user_management.Models.User readerUser, user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(readerUser);

        var readers = user.UserPermissions.Readers.Where(r => r.AuthorId != readerUser.Id).ToList();
        Field[] targetFieldsToRead = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
        readers.Add(new Reader() { Author = Reader.USER, AuthorId = readerUser.Id, IsPermitted = true, Fields = targetFieldsToRead });
        user.UserPermissions.Readers = readers.ToArray();

        await _userCollection.InsertOneAsync(user);

        try
        {
            PartialUser? retrievedUser = await _userRepository.RetrieveById(readerUser.Id, user.Id);

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
                if (targetFieldsToRead.FirstOrDefault<user_management.Models.Field?>(f => f != null && f.Name == fieldName) != null)
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
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        // Assert user deletion
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(readerUser.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_sorting(user_management.Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // Sort users by their created_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].CreatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        user_management.Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPermissions.Readers.ToList();

            Field[] targetFieldsToRead = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.CREATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Field() { IsPermitted = true, Name = user_management.Models.User.CREATED_AT }).ToArray();

            readers.Add(new Reader()
            {
                Author = Reader.USER,
                AuthorId = readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPermissions.Readers = readers.ToArray();
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
                List<user_management.Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, null, limit, currentIteration, user_management.Models.User.CREATED_AT);

                retrievedUsers.ForEach(retrievedUser =>
                {
                    if (previousDt != null) Assert.True(retrievedUser.CreatedAt > previousDt);
                    else previousDt = retrievedUser.CreatedAt;
                });
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_pagination(user_management.Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // sort users by their update_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].UpdatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        user_management.Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPermissions.Readers.ToList();

            Field[] targetFieldsToRead = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.UPDATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Field() { IsPermitted = true, Name = user_management.Models.User.UPDATED_AT }).ToArray();

            readers.Add(new Reader()
            {
                Author = Reader.USER,
                AuthorId = readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPermissions.Readers = readers.ToArray();
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

                List<user_management.Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, null, limit, currentIteration, null);

                Assert.Equal<int>(expectedUserCount, retrievedUsers.Count());

                for (int ru = 0; ru < retrievedUsers.Count(); ru++)
                    Assert.True(retrievedUsers[ru].Id.ToString() == users[(currentIteration * limit) + ru].Id.ToString());
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Retrieve_logicsString(user_management.Models.User[] users)
    {
        DateTime pointerDt = DateTime.UtcNow.AddYears(-1);
        // Sort users by their created_at field
        for (int u = 0; u < users.Length; u++)
        {
            users[u].CreatedAt = new DateTime(pointerDt.Ticks);
            pointerDt = pointerDt.AddHours(1);
        }

        user_management.Models.User readerUser = users.ElementAt(0);
        users = users.Where((u, i) => i != 0).ToArray();

        int limit = 3;
        int maxIteration = (int)Math.Ceiling((double)users.Length / limit);

        // Initiate users' UserPrivileges.Readers property
        for (int i = 0; i < users.Count(); i++)
        {
            var readers = users.ElementAt(i).UserPermissions.Readers.ToList();

            Field[] targetFieldsToRead = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), Faker.Random.Int(2, 4)).ToArray();
            if (targetFieldsToRead.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.CREATED_AT) == null)
                targetFieldsToRead = targetFieldsToRead.Append(new Field() { IsPermitted = true, Name = user_management.Models.User.CREATED_AT }).ToArray();

            readers.Add(new Reader()
            {
                Author = Reader.USER,
                AuthorId = readerUser.Id,
                IsPermitted = true,
                Fields = targetFieldsToRead
            });

            users.ElementAt(i).UserPermissions.Readers = readers.ToArray();
        }

        // Persist users to the db
        for (int i = 0; i < users.Count(); i++)
            await _userCollection.InsertOneAsync(users.ElementAt(i));

        // The actual test
        try
        {
            for (int currentIteration = 0; currentIteration < maxIteration; currentIteration++)
            {
                List<user_management.Models.PartialUser> retrievedUsers = await _userRepository.Retrieve(readerUser.Id, null, limit, currentIteration, user_management.Models.User.CREATED_AT);

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

                        Field[] targetFieldsToRead = users.First(u => u.Id.ToString() == retrievedUser.Id.ToString()).UserPermissions!.Readers.First(r => r.IsPermitted
                            && r.Author == Reader.USER
                            && r.AuthorId == readerUser.Id
                        ).Fields;
                        // If this property exists in chosen fields to read (targetFieldsToRead)
                        if (targetFieldsToRead.FirstOrDefault<user_management.Models.Field?>(f => f != null && f.Name == fieldName) != null)
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
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByIdForAuthenticationHandling(user_management.Models.User user)
    {
        // Success
        user.IsEmailVerified = true;
        user.LoggedOutAt = null;

        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Null(user.LoggedOutAt);
            Assert.True(user.IsEmailVerified);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());

        // Failure
        user.IsEmailVerified = true;
        user.LoggedOutAt = DateTime.UtcNow.AddMinutes(-7);

        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.Null(retrievedUser);
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());

        user.IsEmailVerified = false;
        user.LoggedOutAt = null;

        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthenticationHandling(user.Id);

            Assert.Null(retrievedUser);
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByIdForAuthorizationHandling(user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByIdForAuthorizationHandling(user.Id);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveByIdForAuthorizationHandling(user.Id));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForPasswordChange(user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveUserForPasswordChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForPasswordChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForUsernameChange(user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveUserForUsernameChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForUsernameChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForEmailChange(user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveUserForEmailChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForEmailChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveUserForPhoneNumberChange(user_management.Models.User user)
    {
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveUserForPhoneNumberChange(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveUserForPhoneNumberChange(user.Email));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByClientIdAndCode(user_management.Models.User user)
    {
        string code = "code";
        string clientId = ObjectId.GenerateNewId().ToString();
        DateTime codeExpiresAt = default;
        user.AuthorizingClient = new() { ClientId = clientId, Code = code, CodeChallenge = "CodeChallenge", CodeChallengeMethod = "SHA512", CodeExpiresAt = codeExpiresAt };

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByClientIdAndCode(clientId, code);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.AuthorizingClient.ClientId, retrievedUser.AuthorizingClient!.ClientId);
            Assert.Equal(user.AuthorizingClient.Code, retrievedUser.AuthorizingClient.Code);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First(), new() { { user_management.Models.User.LOGGED_OUT_AT, null } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveByClientIdAndCode(clientId, code));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByRefreshTokenValue(user_management.Models.User user)
    {
        string token = user.AuthorizedClients[0].RefreshToken!.Value!;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByRefreshTokenValue(token);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.AuthorizedClients[0].RefreshToken!.Value, retrievedUser.AuthorizedClients[0].RefreshToken!.Value);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveByRefreshTokenValue(token));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RetrieveByTokenValue(user_management.Models.User user)
    {
        string token = user.AuthorizedClients[0].Token!.Value!;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            user_management.Models.User? retrievedUser = await _userRepository.RetrieveByTokenValue(token);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id.ToString(), retrievedUser.Id.ToString());
            Assert.Equal(user.AuthorizedClients[0].Token!.Value!, retrievedUser.AuthorizedClients[0].Token!.Value!);
            AssertFieldsExpectedValues(user, retrievedUser, new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null(await _userRepository.RetrieveByTokenValue(token));
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Login(user_management.Models.User user)
    {
        // Success
        Assert.NotNull(user.LoggedOutAt);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Login(user.Id);

            Assert.True(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First(), new() { { user_management.Models.User.LOGGED_OUT_AT, null } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecret(user_management.Models.User user)
    {
        string VerificationSecret = "VerificationSecret";

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecret(VerificationSecret, user.Email);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { user_management.Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecretForActivation(user_management.Models.User user)
    {
        user.IsEmailVerified = false;
        string VerificationSecret = "VerificationSecret";

        // Success
        Assert.False(user.IsEmailVerified);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForActivation(VerificationSecret, user.Email);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { user_management.Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());

        // Failure
        user.IsEmailVerified = true;

        Assert.True(user.IsEmailVerified);

        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForActivation(VerificationSecret, user.Email);

            Assert.Null(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First(), new() { });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateVerificationSecretForPasswordChange(user_management.Models.User user)
    {
        string VerificationSecret = "VerificationSecret";

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateVerificationSecretForPasswordChange(VerificationSecret, user.Email);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.VERIFICATION_SECRET, retrievedUser.VerificationSecret }, { user_management.Models.User.VERIFICATION_SECRET_UPDATED_AT, retrievedUser.VerificationSecretUpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Verify(user_management.Models.User user)
    {
        user.IsEmailVerified = false;
        // Success
        Assert.False(user.IsEmailVerified);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Verify(user.Id);

            Assert.True(result);
            AssertFieldsExpectedValues(user, (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First(), new() { { user_management.Models.User.IS_EMAIL_VERIFIED, true } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangePassword(user_management.Models.User user)
    {
        string hashedPassword = "hashedPassword";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangePassword(user.Email, hashedPassword);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.PASSWORD, hashedPassword }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangeUsername(user_management.Models.User user)
    {
        string username = "username";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangeUsername(user.Email, username);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USERNAME, username }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangePhoneNumber(user_management.Models.User user)
    {
        string phoneNumber = "phoneNumber";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangePhoneNumber(user.Email, phoneNumber);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.PHONE_NUMBER, phoneNumber }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void ChangeEmail(user_management.Models.User user)
    {
        string newEmail = "newEmail";
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.ChangeEmail(user.Email, newEmail);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.EMAIL, newEmail }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void Logout(user_management.Models.User user)
    {
        user.LoggedOutAt = null;

        // Success
        Assert.Null(user.LoggedOutAt);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Logout(user.Id);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.LOGGED_OUT_AT, user.LoggedOutAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void RemoveClient(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Readers = new Reader[] {
            new() {
                AuthorId = actor.Id,
                Author = Reader.USER,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = user_management.Models.User.AUTHORIZED_CLIENTS, IsPermitted = true } }
            }
        };

        user.UserPermissions.Updaters = new Updater[] {
            new() {
                AuthorId = actor.Id,
                Author = Updater.USER,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = user_management.Models.User.AUTHORIZED_CLIENTS, IsPermitted = true } }
            }
        };

        // Success
        Assert.True(user.AuthorizedClients.Length >= 1);
        Assert.True(user.UserPermissions.Readers.FirstOrDefault(r => r != null && r.AuthorId == actor.Id && r.IsPermitted && r.Author == Reader.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.AUTHORIZED_CLIENTS && f.IsPermitted) != null) != null);
        Assert.True(user.UserPermissions.Updaters.FirstOrDefault(r => r != null && r.AuthorId == actor.Id && r.IsPermitted && r.Author == Updater.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.AUTHORIZED_CLIENTS && f.IsPermitted) != null) != null);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.RemoveClient(user.Id, user.AuthorizedClients[0].ClientId, actor.Id, false);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.AUTHORIZED_CLIENTS, user.AuthorizedClients.Where(uc => uc.ClientId != user.AuthorizedClients[0].ClientId).ToArray() }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void RemoveAllClients(user_management.Models.User user)
    {
        user.UserPermissions.Readers = new Reader[] {
            new() {
                AuthorId = user.Id,
                Author = Reader.USER,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = user_management.Models.User.AUTHORIZED_CLIENTS, IsPermitted = true } }
            }
        };

        user.UserPermissions.Updaters = new Updater[] {
            new() {
                AuthorId = user.Id,
                Author = Updater.USER,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = user_management.Models.User.AUTHORIZED_CLIENTS, IsPermitted = true } }
            }
        };

        // Success
        Assert.True(user.AuthorizedClients.Length >= 0);
        Assert.True(user.UserPermissions.Readers.FirstOrDefault(r => r != null && r.AuthorId == user.Id && r.IsPermitted && r.Author == Reader.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.AUTHORIZED_CLIENTS && f.IsPermitted) != null) != null);
        Assert.True(user.UserPermissions.Updaters.FirstOrDefault(r => r != null && r.AuthorId == user.Id && r.IsPermitted && r.Author == Updater.USER && r.Fields.Length > 0 && r.Fields.FirstOrDefault(f => f != null && f.Name == user_management.Models.User.AUTHORIZED_CLIENTS && f.IsPermitted) != null) != null);
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.RemoveAllClients(user.Id, user.Id, false);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.AUTHORIZED_CLIENTS, new AuthorizedClient[] { } }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_readsFields(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllReaders = new() { };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllUpdaters = new() { };
        user.UserPermissions.Deleters = new Deleter[] { };
        TokenPrivileges tokenPrivileges = new() { ReadsFields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };
        string clientId = user.AuthorizedClients[0].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPermissions.Readers = user.UserPermissions.Readers.Append(new() { Author = Reader.CLIENT, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_updatesFields(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllReaders = new() { };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllUpdaters = new() { };
        user.UserPermissions.Deleters = new Deleter[] { };
        TokenPrivileges tokenPrivileges = new() { UpdatesFields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };
        string clientId = user.AuthorizedClients[0].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPermissions.Updaters = user.UserPermissions.Updaters.Append(new() { Author = Updater.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_deletes(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllReaders = new() { };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllUpdaters = new() { };
        user.UserPermissions.Deleters = new Deleter[] { };
        TokenPrivileges tokenPrivileges = new() { DeletesUser = true };
        string clientId = user.AuthorizedClients[0].ClientId;

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPermissions.Deleters = user.UserPermissions.Deleters.Append(new() { Author = Deleter.USER, AuthorId = clientId, IsPermitted = true }).ToArray();

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            Assert.True(retrievedUser.UserPermissions.Deleters[0].AuthorId == clientId && retrievedUser.UserPermissions.Deleters[0].IsPermitted);
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_userAlreadyPrivileged(user_management.Models.User user, user_management.Models.User actor)
    {
        string clientId = user.AuthorizedClients[0].ClientId;
        TokenPrivileges tokenPrivileges = new() { ReadsFields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray() };

        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = new Field[] { } }, new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllReaders = new() { };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllUpdaters = new() { };
        user.UserPermissions.Deleters = new Deleter[] { };

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields } };

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void AddTokenPrivilegesToUser_allCases(user_management.Models.User user, user_management.Models.User actor)
    {
        string clientId = user.AuthorizedClients[0].ClientId;
        TokenPrivileges tokenPrivileges = new()
        {
            ReadsFields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetReadableFields(), (int)(Faker.Random.Int(1, 5))).ToArray(),
            UpdatesFields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetMassUpdatableFields(), (int)(Faker.Random.Int(1, 5))).ToArray(),
            DeletesUser = true,
        };

        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = new Field[] { } }, new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllReaders = new() { };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.AllUpdaters = new() { };
        user.UserPermissions.Deleters = new Deleter[] { };

        // Success
        await _userCollection.InsertOneAsync(user);
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.ReadsFields } };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = clientId, IsPermitted = true, Fields = tokenPrivileges.UpdatesFields } };
        user.UserPermissions.Deleters = new Deleter[] { new() { Author = Deleter.USER, AuthorId = clientId, IsPermitted = true } };

        try
        {
            bool? result = await _userRepository.AddTokenPrivilegesToUser(user.Id, actor.Id, clientId, tokenPrivileges);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)));
            await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateAuthorizingClient(user_management.Models.User user)
    {
        string clientId = ObjectId.GenerateNewId().ToString();
        string code = "code";
        AuthorizingClient authorizingClient = new()
        {
            ClientId = clientId,
            Code = code,
            CodeChallenge = "codeChallenge",
            CodeChallengeMethod = "SHA512",
            CodeExpiresAt = DateTime.UtcNow,
            TokenPrivileges = new() { }
        };
        user.AuthorizingClient = null;

        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.UpdateAuthorizingClient(user.Id, authorizingClient);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.AUTHORIZING_CLIENT, authorizingClient }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id))); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void AddAuthorizedClient(user_management.Models.User user)
    {
        string clientId = ObjectId.GenerateNewId().ToString();
        AuthorizedClient authorizedClient = new()
        {
            ClientId = clientId,
            RefreshToken = new()
            {
                ExpirationDate = DateTime.UtcNow,
                Value = "value",
                TokenPrivileges = new() { }
            },
            Token = new()
            {
                ExpirationDate = DateTime.UtcNow,
                Value = "value",
                IsRevoked = false
            }
        };
        user.AuthorizedClients = new AuthorizedClient[] { };

        // Success
        await _userCollection.InsertOneAsync(user);

        user.AuthorizedClients = user.AuthorizedClients.Append(authorizedClient).ToArray();
        try
        {
            bool? result = await _userRepository.AddAuthorizedClient(user.Id, authorizedClient);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.AUTHORIZED_CLIENTS, user.AuthorizedClients }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id))); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(OneUser))]
    public async void UpdateToken(user_management.Models.User user)
    {
        string clientId = ObjectId.GenerateNewId().ToString();
        DateTime? expirationDate = DateTime.UtcNow;
        Token token = new() { ExpirationDate = expirationDate, Value = "newValue", IsRevoked = false };
        user.AuthorizedClients = new AuthorizedClient[] {
            new() {
            ClientId = clientId,
            RefreshToken = new()
            {
                ExpirationDate = DateTime.UtcNow,
                Value = "value",
                TokenPrivileges = new() { }
            },
            Token = new()
            {
                ExpirationDate = DateTime.UtcNow.AddMinutes(-2),
                Value = "value",
                IsRevoked = false
            }
        }};

        // Success
        await _userCollection.InsertOneAsync(user);

        user.AuthorizedClients[0].Token = token;
        try
        {
            bool? result = await _userRepository.UpdateToken(user.Id, clientId, token);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.AUTHORIZED_CLIENTS, user.AuthorizedClients }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id))); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void UpdateUserPrivileges(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };
        user.UserPermissions.Updaters = new Updater[] { new() { Author = Updater.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.USER_PERMISSIONS } } } };

        // Success
        await _userCollection.InsertOneAsync(user);

        user.UserPermissions.Updaters = new Updater[] { new() {
            Author = Updater.USER, AuthorId = ObjectId.GenerateNewId().ToString(),
            IsPermitted = true,
            Fields = Faker.PickRandom<user_management.Models.Field>(user_management.Models.User.GetUpdatableFields(), (int)Faker.Random.Int(1,3)).ToArray()
        }};

        try
        {
            bool? result = await _userRepository.UpdateUserPrivileges(actor.Id, user.Id, user.UserPermissions);

            Assert.True(result);
            user_management.Models.User retrievedUser = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).First();
            AssertFieldsExpectedValues(user, retrievedUser, new() { { user_management.Models.User.USER_PERMISSIONS, user.UserPermissions }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
        }
        finally { await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id))); }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(ManyUsers))]
    public async void Update(user_management.Models.User[] users)
    {
        user_management.Models.User actor = users[0];
        users = users.Where((u, i) => i != 0).ToArray();

        actor.UserPermissions.Readers = new Reader[] { };
        for (int i = 0; i < users.Length; i++)
        {
            user_management.Models.User user = users[i];
            user.UserPermissions.Readers = new Reader[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.FIRST_NAME } } } };
            user.UserPermissions.Updaters = new Updater[] { new() { Author = Reader.USER, AuthorId = actor.Id, IsPermitted = true, Fields = new Field[] { new() { IsPermitted = true, Name = user_management.Models.User.FIRST_NAME } } } };
        }

        // Success
        await _userCollection.InsertOneAsync(actor);
        for (int i = 0; i < users.Length; i++)
            await _userCollection.InsertOneAsync(users[i]);

        try
        {
            bool? result = await _userRepository.Update(actor.Id, null, new Update[] { new() { Field = "LastName", Operation = user_management.Data.Logics.Update.SET, Type = Types.STRING, Value = "test_first_name" } });

            Assert.True(result);
            List<user_management.Models.User> retrievedUsers = (await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Ne("_id", ObjectId.Parse(actor.Id)))).ToList();
            for (int i = 0; i < retrievedUsers.Count; i++)
            {
                user_management.Models.User retrievedUser = retrievedUsers[i];
                Assert.Equal("test_first_name", retrievedUser.FirstName);
                AssertFieldsExpectedValues(users[i], retrievedUser, new() { { user_management.Models.User.FIRST_NAME, "test_first_name" }, { user_management.Models.User.UPDATED_AT, retrievedUser.UpdatedAt } });
            }
        }
        finally { await _userCollection.DeleteManyAsync(Builders<user_management.Models.User>.Filter.Empty); }

        Assert.Empty((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Ne("_id", ObjectId.Parse(actor.Id)))).ToList());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Delete_success(user_management.Models.User user, user_management.Models.User actor)
    {
        user.UserPermissions.Deleters = user.UserPermissions.Deleters.Append(new() { Author = Deleter.USER, AuthorId = actor.Id, IsPermitted = true }).ToArray();
        // Success
        await _userCollection.InsertOneAsync(user);

        try
        {
            bool? result = await _userRepository.Delete(actor.Id, user.Id);

            Assert.True(result);
            Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        }
        finally
        {
            await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)));
            await _userCollection.DeleteOneAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)));
        }

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
    }

    [Theory]
    [MemberData(nameof(TwoUsers))]
    public async void Delete_failure(user_management.Models.User user, user_management.Models.User actor)
    {
        // Failure
        bool? result = await _userRepository.Delete(actor.Id, user.Id);

        Assert.Null(result);

        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault<user_management.Models.User?>());
        Assert.Null((await _userCollection.FindAsync(Builders<user_management.Models.User>.Filter.Eq("_id", ObjectId.Parse(actor.Id)))).FirstOrDefault<user_management.Models.User?>());
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
