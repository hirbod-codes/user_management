namespace user_management.Tests;

using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using user_management.Controllers;
using user_management.Dtos.Token;
using user_management.Models;
using Xunit;

[Collection("Controller")]
public class TokenControllerTests
{
    public ControllerFixture ControllerFixture { get; private set; }

    public TokenControllerTests(ControllerFixture controllerFixture) => ControllerFixture = controllerFixture;

    private TokenController InstantiateController() => new TokenController(ControllerFixture.IMapper.Object, ControllerFixture.IClientRepository.Object, ControllerFixture.IUserRepository.Object, ControllerFixture.IStringHelper.Object, ControllerFixture.IAuthHelper.Object, ControllerFixture.IDateTimeProvider.Object, ControllerFixture.IMongoClient.Object);

    [Fact]
    public async Task Api_token_authorize()
    {
        Faker faker = new("en");
        DateTime dt = DateTime.UtcNow;
        string code = faker.Random.String(40);
        string refreshTokenValue = code;
        string state = faker.Random.String(40);
        string responseType = "code";
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId clientId = ObjectId.GenerateNewId();
        TokenPrivilegesCreateDto scopeDto = new() { };
        TokenPrivileges scope = new() { };
        string codeChallenge = faker.Random.String(40);
        string codeChallengeMethod = faker.PickRandom<string>(new string[] { "SHA256", "SHA512" });
        string redirectUrl = faker.Internet.Url();

        TokenAuthDto tokenAuthDto = new()
        {

            State = state,
            ResponseType = responseType,
            ClientId = clientId.ToString(),
            Scope = scopeDto,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            RedirectUrl = redirectUrl,
        };

        Client client = new();
        User user = new();

        ControllerFixture.IMapper.Setup<TokenPrivileges>(im => im.Map<TokenPrivileges>(scopeDto)).Returns(scope);

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(actorId.ToString()));

        ControllerFixture.IClientRepository.Setup<Task<Client?>>(icr => icr.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(client));

        ControllerFixture.IUserRepository.Setup<Task<User?>>(icr => icr.RetrieveById(It.Is<ObjectId>(id => id.ToString() == actorId.ToString()), It.Is<ObjectId>(id => id.ToString() == actorId.ToString()), false)).Returns(Task.FromResult<User?>(user));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(icr => icr.AddClientById(user, It.Is<ObjectId>(id => id.ToString() == clientId.ToString()), It.Is<ObjectId>(id => id.ToString() == actorId.ToString()), false, scope, dt.AddMonths(TokenController.REFRESH_TOKEN_EXPIRATION_MONTHS), refreshTokenValue, dt.AddMinutes(TokenController.CODE_EXPIRATION_MINUTES), code, codeChallenge, codeChallengeMethod)).Returns(Task.FromResult<bool?>(true));

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.GenerateRandomString(128)).Returns(code);

        ControllerFixture.IDateTimeProvider.Setup<DateTime>(idp => idp.ProvideUtcNow()).Returns(dt);

        var result = await InstantiateController().Authorize(tokenAuthDto);

        Assert.NotNull(result as RedirectResult);
        Assert.True((result as RedirectResult)!.Permanent);
        Assert.Equal(redirectUrl + $"?code={code}&state={state}", (result as RedirectResult)!.Url);
    }

    [Fact]
    public async Task Api_token_token()
    {
        Faker faker = new("en");
        DateTime dt = DateTime.UtcNow;
        ObjectId clientId = ObjectId.GenerateNewId();
        ObjectId businessId = ObjectId.GenerateNewId();
        string refreshToken = faker.Random.String(40);
        string tokenValue = faker.Random.String(40);
        string hashedTokenValue = faker.Random.String(40);
        string grantType = "authorization_code";
        string redirectUrl = faker.Random.String(faker.Random.Int(1, 50));
        string code = faker.Random.String(faker.Random.Int(1, 50));
        string codeChallenge = faker.Random.String(faker.Random.Int(1, 50));
        string codeChallengeMethod = "SHA512";
        string codeVerifier = faker.Random.String(faker.Random.Int(1, 50));
        string hashedCodeVerifier = faker.Random.String(faker.Random.Int(1, 50));

        TokenCreateDto tokenCreateDto = new()
        {
            GrantType = grantType,
            ClientId = clientId.ToString(),
            RedirectUrl = redirectUrl,
            Code = code,
            CodeVerifier = codeVerifier,
        };

        Client client = new() { Id = clientId };
        RefreshToken refreshTokenModel = new()
        {
            ExpirationDate = dt.AddHours(1),
            CodeExpiresAt = dt.AddHours(1),
            CodeChallengeMethod = codeChallengeMethod,
            CodeChallenge = codeChallenge,
            Value = refreshToken,
            TokenPrivileges = new() { }
        };
        User user = new() { Clients = new UserClient[] { new UserClient() { ClientId = clientId, RefreshToken = refreshTokenModel } } };

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(It.Is<string>(s => s == codeVerifier), It.Is<string>(s => s == codeChallengeMethod))).Returns(hashedCodeVerifier);
        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.Base64Decode(codeChallenge)).Returns(hashedCodeVerifier);
        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.GenerateRandomString(128)).Returns(tokenValue);
        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(It.Is<string>(s => s == tokenValue), "SHA512")).Returns(hashedTokenValue);

        ControllerFixture.IClientRepository.Setup<Task<Client?>>(icr => icr.RetrieveByIdAndRedirectUrl(It.Is<ObjectId>(id => id.ToString() == clientId.ToString()), redirectUrl)).Returns(Task.FromResult<Client?>(client));

        Mock<IClientSessionHandle> session = new();
        session.Setup(icsh => icsh.StartTransaction(It.IsAny<TransactionOptions>()));
        session.Setup(icsh => icsh.CommitTransactionAsync(default(CancellationToken)));

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveByClientIdAndCode(It.Is<ObjectId>(id => id.ToString() == clientId.ToString()), code)).Returns(Task.FromResult<User?>(user));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.AddTokenPrivileges(user, It.Is<ObjectId>(id => id.ToString() == clientId.ToString()), refreshTokenModel.TokenPrivileges, session.Object)).Returns(Task.FromResult<bool?>(true));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.AddToken(user, It.Is<ObjectId>(id => id.ToString() == clientId.ToString()), hashedTokenValue, It.IsAny<DateTime>(), session.Object)).Returns(Task.FromResult<bool?>(true));

        ControllerFixture.IMongoClient.Setup<Task<IClientSessionHandle>>(imc => imc.StartSessionAsync(null, default(CancellationToken))).Returns(Task.FromResult<IClientSessionHandle>(session.Object));

        var result = await InstantiateController().Token(tokenCreateDto);

        Assert.Equal<int?>(200, (result.Result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task Api_token_retoken()
    {
        Faker faker = new("en");
        DateTime dt = DateTime.UtcNow;
        ObjectId clientId = ObjectId.GenerateNewId();
        string hashedTokenValue = faker.Random.String(40);
        string tokenValue = faker.Random.String(40);
        string refreshToken = faker.Random.String(40);
        string secret = faker.Random.String(40);
        string hashedSecret = faker.Random.String(40);


        ReTokenDto reTokenDto = new() { ClientId = clientId.ToString(), ClientSecret = secret, RefreshToken = refreshToken };
        Client client = new() { Id = clientId };
        RefreshToken refreshTokenModel = new() { ExpirationDate = dt.AddHours(1), CodeExpiresAt = dt.AddHours(1), IsVerified = true, Value = refreshToken };
        User user = new() { Clients = new UserClient[] { new UserClient() { ClientId = clientId, RefreshToken = refreshTokenModel } } };

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);

        ControllerFixture.IClientRepository.Setup<Task<Client?>>(icr => icr.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(client));

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveByRefreshTokenValue(refreshToken)).Returns(Task.FromResult<User?>(user));

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.GenerateRandomString(128)).Returns(tokenValue);
        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(tokenValue, "SHA512")).Returns(hashedTokenValue);

        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.AddToken(user, clientId, hashedTokenValue, It.IsAny<DateTime>(), null)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateController().ReToken(reTokenDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
        Assert.NotNull(result as OkObjectResult);
        Assert.Equal<int?>(200, (result as OkObjectResult)!.StatusCode);
        Assert.Equal(tokenValue, (result as OkObjectResult)!.Value as string);
    }
}

public class TokenMethodResponse
{
    public string? access_token { get; set; }
    public string? refresh_token { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}