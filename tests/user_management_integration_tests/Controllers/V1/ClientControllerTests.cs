using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Controllers.V1;
using user_management.Data;
using user_management.Dtos.Client;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;

namespace user_management_integration_tests.Controllers.V1;

[CollectionDefinition("ClientControllerTests", DisableParallelization = true)]
public class ClientControllerTestsCollectionDefinition { }

[Collection("ClientControllerTests")]

public class ClientControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private Faker _faker = new();
    private IMongoCollection<User> _userCollection;
    private IMongoCollection<Client> _clientCollection;

    public ClientControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _userCollection = factory.Services.GetService<MongoCollections>()!.Users;
        _clientCollection = factory.Services.GetService<MongoCollections>()!.Clients;
    }

    [Fact]
    public async Task Register_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.REGISTER_CLIENT).Append(new() { Name = StaticData.REGISTER_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);
        // httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        ClientCreateDto dto = new() { RedirectUrl = _faker.Internet.Url() };

        string url = "api/" + ClientController.PATH_POST_REGISTER;

        // When
        HttpResponseMessage response = await httpClient.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ClientRetrieveDto? responseData = await response.Content.ReadFromJsonAsync<ClientRetrieveDto>();

        Assert.NotNull(responseData);
        Assert.Equal(dto.RedirectUrl, responseData.RedirectUrl);
        Assert.NotNull((await _clientCollection.FindAsync(fb.And(
            fb.Eq("_id", ObjectId.Parse(responseData.Id)),
            fb.Eq(Client.REDIRECT_URL, responseData.RedirectUrl),
            fb.Eq(Client.SECRET, new StringHelper().HashWithoutSalt(responseData.Secret!))
        ))).FirstOrDefault());
    }

    [Fact]
    public async Task RetrieveClientPublicInfo_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.READ_CLIENT).Append(new() { Name = StaticData.READ_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).First();

        string url = "api/" + ClientController.PATH_GET_PUBLIC_INFO.Replace("{id}", client.Id);

        // When
        HttpResponseMessage response = await httpClient.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ClientRetrieveDto? responseData = await response.Content.ReadFromJsonAsync<ClientRetrieveDto>();

        Assert.NotNull(responseData);
        Assert.Equal(client.RedirectUrl, responseData.RedirectUrl);
        Assert.Equal(client.UpdatedAt, responseData.UpdatedAt);
        Assert.Equal(client.CreatedAt, responseData.CreatedAt);
    }

    [Fact]
    public async Task Retrieve_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        List<Client> clients = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList();

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients);
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.READ_CLIENT).Append(new() { Name = StaticData.READ_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = Client.FakeClient(out string secret, clients);
        client.ExposedCount = 0;
        client.TokensExposedAt = null;
        await _clientCollection.InsertOneAsync(client);

        string url = "api/" + ClientController.PATH_GET_RETRIEVE.Replace("{secret}", secret);

        // When
        HttpResponseMessage response = await httpClient.GetAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        ClientPublicInfoRetrieveDto? responseData = await response.Content.ReadFromJsonAsync<ClientPublicInfoRetrieveDto>();

        Assert.NotNull(responseData);
        Assert.Equal(client.RedirectUrl, responseData.RedirectUrl);
        Assert.Equal(Math.Floor((double)(client.UpdatedAt.Ticks / 10000)!), Math.Floor((double)(responseData.UpdatedAt?.Ticks / 10000)!));
        Assert.Equal(Math.Floor((double)(client.CreatedAt.Ticks / 10000)!), Math.Floor((double)(responseData.CreatedAt?.Ticks / 10000)!));
    }

    [Fact]
    public async Task Update_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        List<Client> clients = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList();

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients);
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.UPDATE_CLIENT).Append(new() { Name = StaticData.UPDATE_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = Client.FakeClient(out string secret, clients);
        client.ExposedCount = 0;
        client.TokensExposedAt = null;
        await _clientCollection.InsertOneAsync(client);

        ClientPatchDto dto = new()
        {
            Id = client.Id,
            Secret = secret,
            RedirectUrl = _faker.Internet.Url()
        };

        string url = "api/" + ClientController.PATH_PATCH;

        // When
        HttpResponseMessage response = await httpClient.PatchAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotNull((await _clientCollection.FindAsync(fb.And(
            fb.Eq("_id", ObjectId.Parse(client.Id)),
            fb.Eq(Client.REDIRECT_URL, dto.RedirectUrl),
            fb.Eq(Client.SECRET, client.Secret)
        ))).FirstOrDefault());
    }

    [Fact]
    public async Task Delete_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        List<Client> clients = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList();

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients);
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.DELETE_CLIENT).Append(new() { Name = StaticData.DELETE_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = Client.FakeClient(out string secret, clients);
        client.ExposedCount = 0;
        client.TokensExposedAt = null;
        await _clientCollection.InsertOneAsync(client);

        ClientDeleteDto dto = new()
        {
            Id = client.Id,
            Secret = secret,
        };

        string url = "api/" + ClientController.PATH_DELETE.Replace("{secret}", secret);

        // When
        HttpResponseMessage response = await httpClient.DeleteAsync(url);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Null((await _clientCollection.FindAsync(fb.And(
            fb.Eq("_id", ObjectId.Parse(client.Id)),
            fb.Eq(Client.SECRET, client.Secret)
        ))).FirstOrDefault());
    }

    [Fact]
    public async Task UpdateExposedClient_Ok()
    {
        // Given
        var fb = Builders<Client>.Filter;
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        List<Client> clients = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList();

        User u = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients);
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.UPDATE_CLIENT).Append(new() { Name = StaticData.UPDATE_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = Client.FakeClient(out string secret, clients);
        client.ExposedCount = 0;
        client.TokensExposedAt = null;
        await _clientCollection.InsertOneAsync(client);

        ClientExposedDto dto = new()
        {
            ClientId = client.Id,
            Secret = secret,
        };
        var t = JsonContent.Create(dto);

        string url = "api/" + ClientController.PATH_PATCH_EXPOSURE;

        // When
        HttpResponseMessage response = await httpClient.PatchAsJsonAsync(url, dto);

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string responseData = (await response.Content.ReadAsStringAsync()).Trim('"');

        Assert.NotNull((await _clientCollection.FindAsync(fb.And(
            fb.Eq(Client.SECRET, new StringHelper().HashWithoutSalt(responseData)),
            fb.Eq(Client.EXPOSED_COUNT, 1),
            fb.Eq(Client.REDIRECT_URL, client.RedirectUrl),
            fb.Ne<DateTime?>(Client.TOKENS_EXPOSED_AT, null)
        ))).FirstOrDefault());
    }
}
