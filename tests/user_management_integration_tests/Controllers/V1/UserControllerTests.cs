using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Bogus;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using Moq;
using user_management.Data;
using user_management.Data.Logics;
using user_management.Data.MongoDB;
using user_management.Data.Seeders;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;

namespace user_management_integration_tests.Controllers.V1;

[CollectionDefinition("UserControllerTests", DisableParallelization = true)]
public class UserControllerTestsCollectionDefinition { }

[Collection("UserControllerTests")]
public class UserControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    public const string USERS_PASSWORDS = "Pass%w0rd!99";

    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly Faker _faker = new();
    private IMongoCollection<User> _userCollection;
    private IMongoCollection<Client> _clientCollection;

    public UserControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _userCollection = factory.Services.GetService<MongoCollections>()!.Users;
        _clientCollection = factory.Services.GetService<MongoCollections>()!.Clients;
    }

    [Fact]
    public async Task FullNameExistenceCheck_NotFound()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;
        url += "?";
        url += "FirstName=imaginary_first_name&";
        url += "MiddleName=imaginary_first_name&";
        url += "LastName=imaginary_last_name";

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FullNameExistenceCheck_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Or(
            Builders<User>.Filter.Ne<string?>(User.FIRST_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.MIDDLE_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.LAST_NAME, null)
        ))).First();

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;
        url += "?";
        if (user.FirstName != null) url += "FirstName=" + user.FirstName + "&";
        if (user.MiddleName != null) url += "MiddleName=" + user.MiddleName + "&";
        if (user.LastName != null) url += "LastName=" + user.LastName + "&";

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task FullNameExistenceCheck_BadRequest()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Or(
            Builders<User>.Filter.Ne<string?>(User.FIRST_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.MIDDLE_NAME, null),
            Builders<User>.Filter.Ne<string?>(User.LAST_NAME, null)
        ))).First();

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_FULL_NAME_EXISTENCE_CHECK;

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("At least one of the following variables must be provided: firstName, middleName and lastName.", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task UsernameExistenceCheck_NotFound()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_USERNAME_EXISTENCE_CHECK.Replace("{username}", Uri.EscapeDataString("imaginary_username"));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UsernameExistenceCheck_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).First();

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_USERNAME_EXISTENCE_CHECK.Replace("{username}", Uri.EscapeDataString(user.Username));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task EmailExistenceCheck_NotFound()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_EMAIL_EXISTENCE_CHECK.Replace("{email}", Uri.EscapeDataString("imaginary_email@example.com"));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task EmailExistenceCheck_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).First();

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_EMAIL_EXISTENCE_CHECK.Replace("{email}", Uri.EscapeDataString(user.Email));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task PhoneNumberExistenceCheck_NotFound()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // make sure current phone number doesn't exist.
        string phoneNumber = "09999999999";
        await _userCollection.DeleteManyAsync(Builders<User>.Filter.Eq(User.PHONE_NUMBER, phoneNumber));

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK.Replace("{phoneNumber}", Uri.EscapeDataString(Uri.EscapeDataString("09999999999")));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Matches(User.PHONE_NUMBER_REGEX, phoneNumber);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PhoneNumberExistenceCheck_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Ne<string?>(User.PHONE_NUMBER, null))).First();

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK.Replace("{phoneNumber}", Uri.EscapeDataString(user.PhoneNumber!));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Matches(User.PHONE_NUMBER_REGEX, user.PhoneNumber);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_Ok()
    {
        string username = "imaginary_username";
        try
        {
            // Given
            HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

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
            string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_REGISTER;

            // When
            HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create<UserCreateDto>(dto));

            // Then
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            string id = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');

            user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(id)))).FirstOrDefault<User?>();
            Assert.NotNull(user);
            Assert.False(user.IsEmailVerified);
            Reader? reader = user.UserPermissions.Readers.FirstOrDefault<Reader?>(r => r != null && r.Author == Reader.USER && r.AuthorId == id && r.IsPermitted);
            Assert.NotNull(reader);
            reader.Fields.ToList().ForEach(f =>
            {
                Assert.True(f.IsPermitted && User.GetReadableFields().FirstOrDefault(ff => ff.Name == f.Name) != null);
            });

            Updater? updater = user.UserPermissions.Updaters.FirstOrDefault<Updater?>(u => u != null && u.Author == Updater.USER && u.AuthorId.ToString() == id && u.IsPermitted);
            Assert.NotNull(updater);
            updater.Fields.ToList().ForEach(f =>
            {
                Assert.True(f.IsPermitted && User.GetUpdatableFields().FirstOrDefault(ff => ff.Name == f.Name) != null);
            });
        }
        finally { await _userCollection.DeleteManyAsync(Builders<User>.Filter.Eq(User.USERNAME, username)); }
    }

    [Fact]
    public async Task SendVerificationEmail_Ok()
    {
        // Given
        _factory.INotificationHelper.Setup(o => o.SendVerificationMessage(It.IsAny<string>(), It.IsAny<string>()));
        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).First();
        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_SEND_VERIFICATION_EMAIL + "?email=" + user.Email;

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // When
        HttpResponseMessage response = await client.PostAsync(url, null);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.NotEqual(user.VerificationSecret, retrievedUser.VerificationSecret);
        Assert.NotEqual(user.VerificationSecretUpdatedAt, retrievedUser.VerificationSecretUpdatedAt);
    }

    [Fact]
    public async Task Activate_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        User user = (await _userCollection.FindAsync(fb.And(fb.Ne<string?>(User.VERIFICATION_SECRET, null), fb.Eq(User.IS_EMAIL_VERIFIED, false)))).First();
        UpdateResult updateResult = await _userCollection.UpdateOneAsync(fb.Eq("_id", ObjectId.Parse(user.Id)), Builders<User>.Update.Set(User.VERIFICATION_SECRET_UPDATED_AT, DateTime.UtcNow));
        Assert.True(updateResult.IsAcknowledged && updateResult.ModifiedCount == 1);

        Activation dto = new() { Email = user.Email, Password = USERS_PASSWORDS, VerificationSecret = user.VerificationSecret! };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_ACTIVATE;
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.True(retrievedUser.IsEmailVerified);
    }

    [Fact]
    public async Task ChangePassword_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.VerificationSecret = _faker.Random.String2(40);
        user.VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(2);
        user.IsEmailVerified = true;
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.UPDATE_ACCOUNT).Append(new() { Name = StaticData.UPDATE_ACCOUNT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        string newPassword = "New%Password99";
        ChangePassword dto = new() { Email = user.Email, Password = newPassword, PasswordConfirmation = newPassword, VerificationSecret = user.VerificationSecret! };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_CHANGE_PASSWORD;
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The password changed successfully.", message);
        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.True(new StringHelper().DoesHashMatch(retrievedUser.Password, newPassword));
        Assert.NotEqual(user.UpdatedAt, retrievedUser.UpdatedAt);
    }

    [Fact]
    public async Task Login_Ok()
    {
        // Given
        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = true;
        await _userCollection!.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // When
        LoginResult loginResult = await Login(client, user: user);

        // Then
        Assert.NotNull(loginResult);
        Assert.Equal(user.Id.ToString(), loginResult.UserId);
        Assert.IsType<string>(loginResult.Jwt);

        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Null(retrievedUser.LoggedOutAt);
    }

    public static async Task<LoginResult> Login(HttpClient client, string? password = null, IMongoCollection<User>? userCollection = null, User? user = null)
    {
        if (user == null && userCollection == null) throw new ArgumentException("userCollection and user parameters can not be null at the same time.");

        if (user == null)
        {
            user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), password: USERS_PASSWORDS);
            user.IsEmailVerified = true;
            await userCollection!.InsertOneAsync(user);
        }
        Login dto = new() { Email = user.Email, Password = password ?? USERS_PASSWORDS };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_LOGIN;
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<LoginResult>())!;
    }

    [Fact]
    public async Task Logout_Ok()
    {
        // Given
        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_LOGOUT;
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        LoginResult loginResult = await Login(client, userCollection: _userCollection);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        // When
        HttpResponseMessage response = await client.PostAsync(url, null);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(loginResult.UserId)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.NotNull(retrievedUser.LoggedOutAt);

        response = await client.PostAsync(url, null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeUnverifiedEmail_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = false;
        await _userCollection.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        // Make sure new email doesn't exist.
        string newEmail = "new_imaginary_email@example.com";
        await _userCollection.DeleteManyAsync(fb.Eq(User.EMAIL, newEmail));
        ChangeUnverifiedEmail dto = new() { Email = user.Email, NewEmail = newEmail, Password = USERS_PASSWORDS };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_CHANGE_UNVERIFIED_EMAIL;

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The email changed successfully.", message);
        User? retrievedUser = (await _userCollection.FindAsync(fb.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Equal(newEmail, retrievedUser.Email);
    }

    [Fact]
    public async Task ChangeUsername_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.VerificationSecret = _faker.Random.String2(40);
        user.VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(2);
        user.IsEmailVerified = true;
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.UPDATE_ACCOUNT).Append(new() { Name = StaticData.UPDATE_ACCOUNT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        // Make sure new username doesn't exist.
        string newUsername = "newUsername";
        await _userCollection.DeleteManyAsync(fb.Eq(User.USERNAME, newUsername));
        ChangeUsername dto = new() { Email = user.Email, Username = newUsername, VerificationSecret = user.VerificationSecret! };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_CHANGE_USERNAME;

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The username changed successfully.", message);
        User? retrievedUser = (await _userCollection.FindAsync(fb.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Equal(newUsername, retrievedUser.Username);
    }

    [Fact]
    public async Task ChangePhoneNumber_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.VerificationSecret = _faker.Random.String2(40);
        user.VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(2);
        user.IsEmailVerified = true;
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.UPDATE_ACCOUNT).Append(new() { Name = StaticData.UPDATE_ACCOUNT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        // Make sure new phone number doesn't exist.
        string newPhoneNumber = "09999999999";
        await _userCollection.DeleteManyAsync(fb.Eq(User.PHONE_NUMBER, newPhoneNumber));
        ChangePhoneNumber dto = new() { Email = user.Email, PhoneNumber = newPhoneNumber, VerificationSecret = user.VerificationSecret! };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_CHANGE_PHONE_NUMBER;

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Matches(User.PHONE_NUMBER_REGEX, newPhoneNumber);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The phone number changed successfully.", message);
        User? retrievedUser = (await _userCollection.FindAsync(fb.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Equal(newPhoneNumber, retrievedUser.PhoneNumber);
    }

    [Fact]
    public async Task ChangeEmail_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.VerificationSecret = _faker.Random.String2(40);
        user.VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(2);
        user.IsEmailVerified = true;
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.UPDATE_ACCOUNT).Append(new() { Name = StaticData.UPDATE_ACCOUNT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });
        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        // Make sure new email doesn't exist.
        string newEmail = "new_imaginary_email@example.com";
        await _userCollection.DeleteManyAsync(fb.Eq(User.EMAIL, newEmail));
        ChangeEmail dto = new() { Email = user.Email, NewEmail = newEmail, VerificationSecret = user.VerificationSecret! };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_CHANGE_EMAIL;

        // When
        HttpResponseMessage response = await client.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The email changed successfully.", message);
        User? retrievedUser = (await _userCollection.FindAsync(fb.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Equal(newEmail, retrievedUser.Email);
    }

    [Fact]
    public async Task RemoveClient_Ok()
    {
        // Given
        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = true;
        if (user.AuthorizedClients.Length == 0) user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(_faker.PickRandom((await _clientCollection.FindAsync(fc.Empty)).ToList()))).ToArray();
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.DELETE_CLIENT).Append(new() { Name = StaticData.DELETE_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_REMOVE_CLIENT + "?clientId=" + user.AuthorizedClients[0].ClientId.ToString() + "&userId=" + user.Id.ToString();

        // When
        HttpResponseMessage response = await client.PostAsync(url, null);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("The client removed successfully.", message);

        User? retrievedUser = (await _userCollection.FindAsync(fb.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Null(retrievedUser.AuthorizedClients.FirstOrDefault(uc => uc != null && uc.ClientId == user.AuthorizedClients[0].ClientId));
    }

    [Fact]
    public async Task RemoveClients_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = true;
        if (user.AuthorizedClients.Length == 0) user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(_faker.PickRandom((await _clientCollection.FindAsync(fc.Empty)).ToList()))).ToArray();
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.DELETE_CLIENTS).Append(new() { Name = StaticData.DELETE_CLIENTS, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_POST_REMOVE_CLIENTS + "?userId=" + user.Id.ToString();

        // When
        HttpResponseMessage response = await client.PostAsync(url, null);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string message = (await response.Content.ReadAsStringAsync()).TrimStart('\"').TrimEnd('\"');
        Assert.Equal("All of the clients removed successfully.", message);

        User? retrievedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault();
        Assert.NotNull(retrievedUser);
        Assert.Empty(retrievedUser.AuthorizedClients);
    }

    [Fact]
    public async Task RetrieveById_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = true;
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.READ_ACCOUNT).Append(new() { Name = StaticData.READ_ACCOUNT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        var readers = user.UserPermissions.Readers.Where(r => r.AuthorId != user.Id).ToList();
        Field[] targetFieldsToRead = _faker.PickRandom(User.GetReadableFields(), _faker.Random.Int(2, 4)).Where(f => f.Name != "_id").ToArray();
        readers.Add(new() { Author = Reader.USER, AuthorId = user.Id, IsPermitted = true, Fields = targetFieldsToRead });
        user.UserPermissions.Readers = readers.ToArray();

        UpdateResult updateResult = await _userCollection.UpdateOneAsync(fb.Eq("_id", ObjectId.Parse(user.Id)), Builders<User>.Update.Set(User.USER_PERMISSIONS, user.UserPermissions));
        Assert.True(updateResult.IsAcknowledged && updateResult.MatchedCount == 1 && updateResult.ModifiedCount <= 1);

        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_USER.Replace("{userId}", Uri.EscapeDataString(user.Id.ToString()));

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Dictionary<string, object?>? retrievedUser = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>();
        Assert.NotNull(retrievedUser);

        // Adding one because _id field is always added to the result. 
        Assert.Equal(retrievedUser.Count, targetFieldsToRead.Length + 1);
        Assert.True(retrievedUser.TryGetValue("_id", out object? userId));
        Assert.NotNull(userId);
        Assert.Equal(user.Id.ToString(), retrievedUser["_id"]!.ToString());
        targetFieldsToRead.ToList().ForEach(f => Assert.True(retrievedUser.ContainsKey(f.Name)));
    }

    [Fact]
    public async Task RetrieveClients_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.IsEmailVerified = true;
        if (user.AuthorizedClients.Length == 0) user.AuthorizedClients = user.AuthorizedClients.Append(AuthorizedClient.FakeAuthorizedClient(_faker.PickRandom((await _clientCollection.FindAsync(fc.Empty)).ToList()))).ToArray();
        user.Privileges = user.Privileges.Where(p => p.Name != StaticData.READ_CLIENTS).Append(new() { Name = StaticData.READ_CLIENTS, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(user);

        LoginResult loginResult = await Login(client, user: user);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_USER_AUTHORIZED_CLIENTS;

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Dictionary<string, object?>[]? authorizedClients = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>[]>();
        Assert.NotNull(authorizedClients);
        Assert.Equal(authorizedClients.Length, user.AuthorizedClients.Length);
        user.AuthorizedClients.ToList().ForEach(uc =>
        {
            Assert.NotNull(authorizedClients.FirstOrDefault(ac =>
            {
                if (ac == null) return false;
                if (!ac.TryGetValue(AuthorizedClient.CLIENT_ID, out object? clientId)) return false;
                if (clientId == null) return false;
                if (ac[AuthorizedClient.CLIENT_ID]!.ToString() != uc.ClientId.ToString()) return false;
                return true;
            }, null));
        });
    }

    [Fact]
    public async Task Retrieve_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fbu = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fbc = Builders<Client>.Filter;

        User actor = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        actor.IsEmailVerified = true;
        actor.Privileges = new Privilege[] { new() { Name = StaticData.READ_ACCOUNT, Value = true }, new() { Name = StaticData.READ_ACCOUNTS, Value = true } };
        await _userCollection.InsertOneAsync(actor);

        LoginResult loginResult = await Login(client, user: actor);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        User user1 = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        user1.UserPermissions.Readers = user1.UserPermissions.Readers
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Reader.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = User.GetReadableFields().ToArray()
            })
            .ToArray();
        await _userCollection.InsertOneAsync(user1);

        User user2 = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        user2.UserPermissions.Readers = user2.UserPermissions.Readers
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Reader.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = User.GetReadableFields().ToArray()
            })
            .ToArray();
        await _userCollection.InsertOneAsync(user2);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_GET_USERS
            .Replace("{limit}", Uri.EscapeDataString("5"))
            .Replace("{iteration}", Uri.EscapeDataString("0"))
            .Replace("/{sortBy?}", Uri.EscapeDataString(""))
            .Replace("/{ascending?}", Uri.EscapeDataString(""))
            + "?filtersString=" + JsonSerializer.Serialize(new Filter() { Operation = Filter.OR, Filters = new Filter[] { new() { Field = User.USERNAME, Operation = Filter.EQ, Type = Types.STRING, Value = user1.Username }, new() { Field = User.EMAIL, Operation = Filter.EQ, Type = Types.STRING, Value = user2.Email } } })
        ;

        // When
        HttpResponseMessage response = await client.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Dictionary<string, object?>[]? retrievedUsers = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>[]>();
        Assert.NotNull(retrievedUsers);

        Assert.Equal(2, retrievedUsers.Length);

        Assert.True(retrievedUsers[0].TryGetValue("_id", out object? user1Id));
        Assert.NotNull(user1Id);
        Assert.NotNull((new User[] { user1, user2 }).FirstOrDefault(u => u.Id.ToString() == retrievedUsers[0]["_id"]!.ToString()));

        Assert.True(retrievedUsers[1].TryGetValue("_id", out object? user2Id));
        Assert.NotNull(user2Id);
        Assert.NotNull((new User[] { user1, user2 }).FirstOrDefault(u => u.Id.ToString() == retrievedUsers[1]["_id"]!.ToString()));
    }

    [Fact]
    public async Task Update_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fbu = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fbc = Builders<Client>.Filter;

        User actor = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        actor.IsEmailVerified = true;
        actor.Privileges = new Privilege[] {
            new() { Name = StaticData.READ_ACCOUNT, Value = true },
            new() { Name = StaticData.READ_ACCOUNTS, Value = true },
            new() { Name = StaticData.UPDATE_ACCOUNT, Value = true },
            new() { Name = StaticData.UPDATE_ACCOUNTS, Value = true }
        };
        await _userCollection.InsertOneAsync(actor);

        LoginResult loginResult = await Login(client, user: actor);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        User user1 = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        user1.UserPermissions.Readers = user1.UserPermissions.Readers
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Reader.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = User.USERNAME, IsPermitted = true }, new() { Name = User.LAST_NAME, IsPermitted = true } }
            })
            .ToArray();
        user1.UserPermissions.Updaters = user1.UserPermissions.Updaters
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Updater.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = User.LAST_NAME, IsPermitted = true } }
            })
            .ToArray();
        await _userCollection.InsertOneAsync(user1);

        User user2 = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        user2.UserPermissions.Readers = user2.UserPermissions.Readers
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Reader.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = User.USERNAME, IsPermitted = true }, new() { Name = User.LAST_NAME, IsPermitted = true } }
            })
            .ToArray();
        user2.UserPermissions.Updaters = user2.UserPermissions.Updaters
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Updater.USER,
                AuthorId = actor.Id,
                IsPermitted = true,
                Fields = new Field[] { new() { Name = User.LAST_NAME, IsPermitted = true } }
            })
            .ToArray();
        await _userCollection.InsertOneAsync(user2);

        string lastName = "fake_last_name";
        UserPatchDto dto = new()
        {
            Filters = new Filter()
            {
                Operation = Filter.OR,
                Filters = new Filter[] { new() { Field = User.USERNAME, Operation = Filter.EQ, Type = Types.STRING, Value = user1.Username }, new() { Field = User.USERNAME, Operation = Filter.EQ, Type = Types.STRING, Value = user2.Username } },
            },
            Updates = new Update[] { new() { Field = User.LAST_NAME, Operation = Update.SET, Type = Types.STRING, Value = lastName } }
        };

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_PATCH_USERS;

        // When
        HttpResponseMessage response = await client.PatchAsync(url, JsonContent.Create(dto));
        var t = await response.Content.ReadAsStringAsync();

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Ok()
    {
        // Given
        HttpClient client = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fbu = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fbc = Builders<Client>.Filter;

        User actor = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        actor.IsEmailVerified = true;
        actor.Privileges = new Privilege[] { new() { Name = StaticData.READ_ACCOUNT, Value = true }, new() { Name = StaticData.DELETE_ACCOUNT, Value = true } };
        await _userCollection.InsertOneAsync(actor);

        LoginResult loginResult = await Login(client, user: actor);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        User user = new UserSeeder().FakeUser(ObjectId.GenerateNewId().ToString(), (await _userCollection.FindAsync(fbu.Empty)).ToList(), (await _clientCollection.FindAsync(fbc.Empty)).ToList(), password: USERS_PASSWORDS);
        user.UserPermissions.Deleters = user.UserPermissions.Deleters
            .Where(r => r.AuthorId.ToString() != actor.Id.ToString())
            .Append(new()
            {
                Author = Deleter.USER,
                AuthorId = actor.Id,
                IsPermitted = true
            })
            .ToArray();
        await _userCollection.InsertOneAsync(user);

        string url = "api/" + user_management.Controllers.V1.UserController.PATH_DELETE_USER + "?id=" + user.Id.ToString();

        // When
        HttpResponseMessage response = await client.DeleteAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null((await _userCollection.FindAsync(fbu.Eq("_id", ObjectId.Parse(user.Id)))).FirstOrDefault());
    }
}
