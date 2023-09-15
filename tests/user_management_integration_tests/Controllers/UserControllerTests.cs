using System.Net;
using System.Net.Http.Json;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Data;
using user_management.Dtos.User;
using user_management.Models;

namespace user_management_integration_tests.Controllers;

[CollectionDefinition("UserControllerTests", DisableParallelization = true)]
public class UserControllerTestsCollectionDefinition { }

[Collection("UserControllerTests")]
public class UserControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly IMongoClient _mongoClient = null!;
    private readonly MongoContext _mongoContext = null!;
    private readonly CustomWebApplicationFactory<Program> _fixture;
    private Faker _faker = new();
    private IMongoCollection<User> _userCollection;
    private IMongoDatabase _database;

    public UserControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _fixture = factory;

        _mongoClient = factory.Services.GetService<IMongoClient>()!;
        _mongoContext = factory.Services.GetService<MongoContext>()!;

        _database = _mongoClient.GetDatabase(_mongoContext.DatabaseName);
        _userCollection = _database.GetCollection<User>(_mongoContext.Collections.Users);

        _mongoContext.Initialize().Wait();
        (new Seeder(_mongoContext)).Seed().Wait();
    }

    [Fact]
    public async Task FullNameExistenceCheck_NotFound()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;
        url += "?";
        url += "FirstName=imaginary_first_name&";
        url += "MiddleName=imaginary_first_name&";
        url += "LastName=imaginary_last_name";

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullNameExistenceCheck_Ok()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Or(
            Builders<User>.Filter.Ne<string?>(User.FIRST_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.MIDDLE_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.LAST_NAME, null)
        ))).First();

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;
        url += "?";
        if (user.FirstName != null) url += "FirstName=" + user.FirstName + "&";
        if (user.MiddleName != null) url += "MiddleName=" + user.MiddleName + "&";
        if (user.LastName != null) url += "LastName=" + user.LastName + "&";

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FullNameExistenceCheck_BadRequest()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Or(
            Builders<User>.Filter.Ne<string?>(User.FIRST_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.MIDDLE_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.LAST_NAME, null)
        ))).First();

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("\"At least one of the following variables must be provided: firstName, middleName and lastName.\"", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UsernameExistenceCheck_NotFound()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_USERNAME_EXISTENCE_CHECK.Replace("{username}", "imaginary_username");

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UsernameExistenceCheck_Ok()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).First();

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_USERNAME_EXISTENCE_CHECK.Replace("{username}", user.Username);

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EmailExistenceCheck_NotFound()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_EMAIL_EXISTENCE_CHECK.Replace("{email}", "imaginary_email@example.com");

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EmailExistenceCheck_Ok()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).First();

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_EMAIL_EXISTENCE_CHECK.Replace("{email}", user.Email);

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PhoneNumberExistenceCheck_NotFound()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK.Replace("{phoneNumber}", "09999999999");

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PhoneNumberExistenceCheck_Ok()
    {
        // Arrange
        var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Ne<string?>(User.PHONE_NUMBER, null))).First();

        string url = "api/" + user_management.Controllers.UserController.PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK.Replace("{phoneNumber}", user.PhoneNumber);

        // Act
        var response = await client.GetAsync(url);

        // Assert
        Assert.Matches("^[a-z0-9 +-{)(}]{11,}$", user.PhoneNumber);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_Ok()
    {
        string username = "imaginary_username";
        try
        {
            // Arrange
            var client = _fixture.CreateClient(new() { AllowAutoRedirect = false });

            UserCreateDto dto = new()
            {
                Email = _faker.Internet.ExampleEmail(),
                Password = "Pass0%99",
                Username = username
            };

            User? user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq<string?>(User.USERNAME, dto.Username))).FirstOrDefault<User?>();
            Assert.Null(user);
            user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq<string?>(User.EMAIL, dto.Email))).FirstOrDefault<User?>();
            Assert.Null(user);

            // Act
            string url = "api/" + user_management.Controllers.UserController.PATH_POST_REGISTER;
            var response = await client.PostAsync(url, JsonContent.Create<UserCreateDto>(dto));

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string id = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');

            user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)))).FirstOrDefault<User?>();
            Assert.NotNull(user);
            Assert.False(user.IsVerified);
            Reader? reader = user.UserPrivileges.Readers.FirstOrDefault<Reader?>(r => r != null && r.Author == Reader.USER && r.AuthorId.ToString() == id && r.IsPermitted);
            Assert.NotNull(reader);
            reader.Fields.ToList().ForEach(f =>
            {
                Assert.True(f.IsPermitted && User.GetReadableFields().FirstOrDefault(ff => ff.Name == f.Name) != null);
            });

            Updater? updater = user.UserPrivileges.Updaters.FirstOrDefault<Updater?>(u => u != null && u.Author == Updater.USER && u.AuthorId.ToString() == id && u.IsPermitted);
            Assert.NotNull(updater);
            updater.Fields.ToList().ForEach(f =>
            {
                Assert.True(f.IsPermitted && User.GetUpdatableFields().FirstOrDefault(ff => ff.Name == f.Name) != null);
            });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<User>.Filter.Eq(User.USERNAME, username)); }
    }
}
