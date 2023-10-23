using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Controllers;
using user_management.Data;
using user_management.Dtos.Token;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;

namespace user_management_integration_tests.Controllers;

[CollectionDefinition("TokenControllerTests", DisableParallelization = true)]
public class TokenControllerTestsCollectionDefinition { }

[Collection("TokenControllerTests")]

public class TokenControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private Faker _faker = new();
    private IMongoCollection<User> _userCollection;
    private IMongoCollection<Client> _clientCollection;

    public TokenControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        _userCollection = factory.Services.GetService<MongoCollections>()!.Users;
        _clientCollection = factory.Services.GetService<MongoCollections>()!.Clients;
    }

    [Fact]
    public async Task Authorize_Ok()
    {
        // Given
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });

        FilterDefinitionBuilder<User> fb = Builders<User>.Filter;
        FilterDefinitionBuilder<Client> fc = Builders<Client>.Filter;

        User u = User.FakeUser((await _userCollection.FindAsync(fb.Empty)).ToList(), (await _clientCollection.FindAsync(fc.Empty)).ToList());
        u.VerificationSecret = _faker.Random.String2(40);
        u.VerificationSecretUpdatedAt = DateTime.UtcNow.AddMinutes(2);
        u.IsEmailVerified = true;
        u.Privileges = u.Privileges.Where(p => p.Name != StaticData.AUTHORIZE_CLIENT).Append(new() { Name = StaticData.AUTHORIZE_CLIENT, Value = true }).ToArray();
        await _userCollection.InsertOneAsync(u);

        LoginResult loginResult = await UserControllerTests.Login(httpClient, user: u);
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginResult.Jwt);

        Client client = (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).First();
        TokenAuthDto dto = new()
        {
            ClientId = client.Id.ToString(),
            CodeChallenge = _faker.Random.String2(128),
            CodeChallengeMethod = "SHA512",
            RedirectUrl = client.RedirectUrl,
            Scope = new() { DeletesUser = true },
            ResponseType = "code",
            State = _faker.Random.String2(40)
        };

        string url = "api/" + TokenController.PATH_POST_AUTHORIZE;

        // When
        HttpResponseMessage response = await httpClient.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.MovedPermanently, response.StatusCode);

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", ObjectId.Parse(loginResult.UserId)))).First();
        Assert.NotNull(user.AuthorizingClient);
        Assert.Equal(user.AuthorizingClient.ClientId.ToString(), dto.ClientId);
        Assert.NotEqual(0, user.AuthorizingClient.Code.Length);
        Assert.Equal(user.AuthorizingClient.CodeChallenge, dto.CodeChallenge);
        Assert.Equal(user.AuthorizingClient.CodeChallengeMethod, dto.CodeChallengeMethod);
        Assert.True(user.AuthorizingClient.CodeExpiresAt > DateTime.UtcNow.AddSeconds(40));
    }

    [Fact]
    public async Task VerifyAndGenerateTokens_Ok()
    {
        // Given
        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });
        string code = _faker.Random.String2(128);
        string codeVerifier = _faker.Random.String2(128);
        string codeChallengeMethod = "SHA512";
        string codeChallenge = new StringHelper().Base64Encode(new StringHelper().HashWithoutSalt(codeVerifier, codeChallengeMethod)!);
        List<Client> clients = (await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.EXPOSED_COUNT, 0))).ToList();

        User user = User.FakeUser((await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients);
        user.AuthorizedClients = user.AuthorizedClients.Where(ac => ac.ClientId.ToString() != clients[0].Id.ToString()).ToArray();
        user.AuthorizingClient = new()
        {
            ClientId = clients[0].Id,
            Code = code,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            CodeExpiresAt = DateTime.UtcNow.AddSeconds(60),
            TokenPrivileges = new() { DeletesUser = true }
        };
        await _userCollection.InsertOneAsync(user);

        TokenCreateDto dto = new()
        {
            ClientId = clients[0].Id.ToString(),
            Code = code,
            CodeVerifier = codeVerifier,
            GrantType = "authorization_code",
            RedirectUrl = clients[0].RedirectUrl,
        };

        string url = "api/" + TokenController.PATH_POST_TOKEN_VERIFICATION;

        // When
        HttpResponseMessage response = await httpClient.PostAsync(url, JsonContent.Create(dto));

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TokenRetrieveDto? responseData = await response.Content.ReadFromJsonAsync<TokenRetrieveDto>();
        Assert.NotNull(responseData);

        user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", user.Id))).First();
        AuthorizedClient? userAuthorizedClient = user.AuthorizedClients.FirstOrDefault(ac => ac != null && ac.ClientId.ToString() == clients[0].Id.ToString());
        Assert.NotNull(userAuthorizedClient);
        Assert.True(userAuthorizedClient.RefreshToken.TokenPrivileges.DeletesUser);
        Assert.Equal(userAuthorizedClient.RefreshToken.Value, new StringHelper().HashWithoutSalt(responseData.RefreshToken));
        Assert.Equal(userAuthorizedClient.Token.Value, new StringHelper().HashWithoutSalt(responseData.Token));
    }

    [Fact]
    public async Task ReToken_Ok()
    {
        // Given
        var fbu = Builders<User>.Filter;
        var fbc = Builders<Client>.Filter;
        Client client = Client.FakeClient(out string secret, (await _clientCollection.FindAsync(fbc.Empty)).ToList());
        client.ExposedCount = 0;
        client.TokensExposedAt = null;
        await _clientCollection.InsertOneAsync(client);

        string tokenValue, refreshTokenValue, hashedTokenValue, hashedRefreshTokenValue;
        do
        {
            tokenValue = new StringHelper().GenerateRandomString(128);
            refreshTokenValue = new StringHelper().GenerateRandomString(128);
            hashedTokenValue = new StringHelper().HashWithoutSalt(tokenValue)!;
            hashedRefreshTokenValue = new StringHelper().HashWithoutSalt(refreshTokenValue)!;
        } while ((await _userCollection.FindAsync(fbu.Or(
            fbu.Eq(User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.TOKEN + "." + Token.VALUE, hashedTokenValue),
            fbu.Eq(User.AUTHORIZED_CLIENTS + "." + AuthorizedClient.REFRESH_TOKEN + "." + RefreshToken.VALUE, hashedRefreshTokenValue))
        )).FirstOrDefault() != null);

        User user = (await _userCollection.FindAsync(Builders<User>.Filter.Eq(User.IS_EMAIL_VERIFIED, true))).First();
        AuthorizedClient authorizedClient = new()
        {
            ClientId = client.Id,
            RefreshToken = new()
            {
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
                TokenPrivileges = new() { DeletesUser = true },
                Value = hashedRefreshTokenValue
            },
            Token = new()
            {
                IsRevoked = false,
                Value = hashedTokenValue,
                ExpirationDate = DateTime.UtcNow.AddMinutes(5),
            }
        };
        user.AuthorizedClients = user.AuthorizedClients
            .Append(authorizedClient)
            .ToArray();
        ReplaceOneResult replaceOneResult = await _userCollection.ReplaceOneAsync(Builders<User>.Filter.Eq("_id", user.Id), user);
        Assert.True(replaceOneResult.IsAcknowledged);
        Assert.Equal(1, replaceOneResult.MatchedCount);
        Assert.Equal(1, replaceOneResult.ModifiedCount);

        ReTokenDto dto = new()
        {
            ClientId = client.Id.ToString(),
            ClientSecret = secret,
            RefreshToken = refreshTokenValue
        };

        HttpClient httpClient = _factory.CreateClient(new() { AllowAutoRedirect = false });
        string url = "api/" + TokenController.PATH_POST_RETOKEN;

        // When
        HttpResponseMessage response = await httpClient.PostAsync(url, JsonContent.Create(dto));
        string newTokenValue = (await response.Content.ReadAsStringAsync()).TrimStart('"').TrimEnd('"');

        // Then
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.NotEqual(newTokenValue, tokenValue);

        User updatedUser = (await _userCollection.FindAsync(Builders<User>.Filter.Eq("_id", user.Id))).First();

        Assert.Equal(new StringHelper().HashWithoutSalt(newTokenValue), updatedUser.AuthorizedClients.First(c => c.ClientId.ToString() == authorizedClient.ClientId.ToString()).Token.Value);
        Assert.NotEqual(
            user.AuthorizedClients.First(c => c.ClientId.ToString() == authorizedClient.ClientId.ToString()).Token.ExpirationDate,
            updatedUser.AuthorizedClients.First(c => c.ClientId.ToString() == authorizedClient.ClientId.ToString()).Token.ExpirationDate
        );
    }
}
