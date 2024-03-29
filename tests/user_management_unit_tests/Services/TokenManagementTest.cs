using System.Security.Authentication;
using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using user_management.Data;
using user_management.Dtos.Token;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;
using user_management.Services.Data.Client;

namespace user_management_unit_tests.Services;

[Collection("Service")]
public class TokenManagementTest
{
    public ServiceFixture Fixture { get; private set; }

    public TokenManagementTest(ServiceFixture serviceFixture) => Fixture = serviceFixture;

    private TokenManagement InstantiateService() => new TokenManagement(Fixture.IStringHelper.Object, Fixture.IClientRepository.Object, Fixture.IUserRepository.Object, Fixture.IDateTimeProvider.Object, Fixture.IMongoClient.Object, Fixture.IAuthenticatedByJwt.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void Authorize_clientId_Failure()
    {
        ArgumentException argumentException = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().Authorize(
                "clientId",
                "redirectUrl",
                "codeChallenge",
                "codeChallengeMethod",
                new()
            ));
        Assert.Equal("clientId", argumentException.Message);
    }

    [Fact]
    public async void Authorize_authentication_Failure()
    {
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Throws<AuthenticationException>();
        await Assert.ThrowsAsync<AuthenticationException>(async () => await InstantiateService().Authorize(
                ObjectId.GenerateNewId().ToString(),
                "redirectUrl",
                "codeChallenge",
                "codeChallengeMethod",
                new()
            ));
    }

    [Fact]
    public async void Authorize_clientExistenceAndExposure_Failure()
    {
        User user = new();
        ObjectId clientId = ObjectId.GenerateNewId();
        string redirectUrl = "redirectUrl";

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(user));
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(null));

        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                "codeChallenge",
                "codeChallengeMethod",
                new()
            ));
        Assert.Equal("client", dataNotFoundException.Message);

        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(new() { ExposedCount = 3 }));

        await Assert.ThrowsAsync<BannedClientException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                "codeChallenge",
                "codeChallengeMethod",
                new()
            ));
    }

    [Fact]
    public async void Authorize_tokenPrivileges_Failure()
    {
        User user = new() { Privileges = Faker.PickRandom<Privilege>(StaticData.Privileges, 1).ToArray() };
        ObjectId clientId = ObjectId.GenerateNewId();
        string redirectUrl = "redirectUrl";

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(user));
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(new()));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                "codeChallenge",
                "codeChallengeMethod",
                new() { Privileges = StaticData.Privileges.Where(p => p.Name != user.Privileges[0].Name).ToArray() }
            ));
    }

    [Fact]
    public async void Authorize_codeGeneration_Failure()
    {
        User user = new() { Privileges = Faker.PickRandom<Privilege>(StaticData.Privileges, 1).ToArray() };
        ObjectId clientId = ObjectId.GenerateNewId();
        string redirectUrl = "redirectUrl";
        TokenPrivileges scope = new() { Privileges = StaticData.Privileges.Where(p => p.Name == user.Privileges[0].Name).ToArray() };

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(user));
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(new()));
        Fixture.IUserRepository.Setup(o => o.UpdateAuthorizingClient(user.Id, It.Is<AuthorizingClient>(ac => ac.ClientId.ToString() == clientId.ToString()))).Throws<DuplicationException>();

        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                "codeChallenge",
                "codeChallengeMethod",
                scope
            ));
    }

    [Fact]
    public async void Authorize_database_Failure()
    {
        User user = new() { Privileges = Faker.PickRandom<Privilege>(StaticData.Privileges, 1).ToArray() };
        ObjectId clientId = ObjectId.GenerateNewId();
        string redirectUrl = "redirectUrl";

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(user));
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(new()));
        string code = "code";
        string codeChallenge = "codeChallenge";
        string codeChallengeMethod = "codeChallengeMethod";
        DateTime codeExpiresAt = DateTime.UtcNow;
        DateTime expirationDate = DateTime.UtcNow;
        TokenPrivileges scope = new() { Privileges = StaticData.Privileges.Where(p => p.Name == user.Privileges[0].Name).ToArray() };

        Fixture.IDateTimeProvider.SetupSequence(o => o.ProvideUtcNow()).Returns(codeExpiresAt).Returns(expirationDate);
        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(code);
        Fixture.IUserRepository.Setup(o => o.UpdateAuthorizingClient(user.Id, It.Is<AuthorizingClient>(ac =>
            ac.ClientId.ToString() == clientId.ToString() &&
            ac.TokenPrivileges == scope &&
            ac.Code == code &&
            ac.CodeChallenge == codeChallenge &&
            ac.CodeChallengeMethod == codeChallengeMethod &&
            ac.CodeExpiresAt == codeExpiresAt.AddMinutes(TokenManagement.CODE_EXPIRATION_MINUTES)
        ))).Returns(Task.FromResult<bool?>(false));

        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                codeChallenge,
                codeChallengeMethod,
                scope
            ));

        Fixture.IUserRepository.Setup(o => o.UpdateAuthorizingClient(user.Id, It.Is<AuthorizingClient>(ac =>
            ac.ClientId.ToString() == clientId.ToString() &&
            ac.TokenPrivileges == scope &&
            ac.Code == code &&
            ac.CodeChallenge == codeChallenge &&
            ac.CodeChallengeMethod == codeChallengeMethod &&
            ac.CodeExpiresAt == codeExpiresAt.AddMinutes(TokenManagement.CODE_EXPIRATION_MINUTES)
        ))).Returns(Task.FromResult<bool?>(null));

        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                "codeChallenge",
                "codeChallengeMethod",
                scope
            ));
    }

    [Fact]
    public async void Authorize()
    {
        User user = new() { Privileges = Faker.PickRandom<Privilege>(StaticData.Privileges, 1).ToArray() };
        ObjectId clientId = ObjectId.GenerateNewId();
        string redirectUrl = "redirectUrl";

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(user));
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, redirectUrl)).Returns(Task.FromResult<Client?>(new()));
        string code = "code";
        string codeChallenge = "codeChallenge";
        string codeChallengeMethod = "codeChallengeMethod";
        DateTime codeExpiresAt = DateTime.UtcNow;
        DateTime expirationDate = DateTime.UtcNow;
        TokenPrivileges scope = new() { Privileges = StaticData.Privileges.Where(p => p.Name == user.Privileges[0].Name).ToArray() };

        Fixture.IDateTimeProvider.SetupSequence(o => o.ProvideUtcNow()).Returns(codeExpiresAt).Returns(expirationDate);
        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(code);
        Fixture.IUserRepository.Setup(o => o.UpdateAuthorizingClient(user.Id, It.Is<AuthorizingClient>(ac =>
            ac.ClientId.ToString() == clientId.ToString() &&
            ac.TokenPrivileges == scope &&
            ac.Code == code &&
            ac.CodeChallenge == codeChallenge &&
            ac.CodeChallengeMethod == codeChallengeMethod &&
            ac.CodeExpiresAt == codeExpiresAt.AddMinutes(TokenManagement.CODE_EXPIRATION_MINUTES)
        ))).Returns(Task.FromResult<bool?>(true));

        Assert.Equal(code, await InstantiateService().Authorize(
                clientId.ToString(),
                redirectUrl,
                codeChallenge,
                codeChallengeMethod,
                scope
            ));
    }

    [Fact]
    public async void VerifyAndGenerateTokens_clientId_Failure()
    {
        await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().VerifyAndGenerateTokens(new() { ClientId = "clientId" }));
    }

    [Fact]
    public async void VerifyAndGenerateTokens_client_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        TokenCreateDto dto = new() { ClientId = clientId.ToString(), RedirectUrl = Faker.Internet.Url() };
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl)).Returns(Task.FromResult<Client?>(null));

        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("client", dataNotFoundException.Message);

        Client client = new() { ExposedCount = 3 };
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl)).Returns(Task.FromResult<Client?>(client));

        await Assert.ThrowsAsync<BannedClientException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
    }

    [Fact]
    public async void VerifyAndGenerateTokens_verification_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        TokenCreateDto dto = new() { ClientId = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Code = "code" };
        Client client = new() { Id = clientId };
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl)).Returns(Task.FromResult<Client?>(client));
        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(DateTime.UtcNow);

        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(null));
        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("user", dataNotFoundException.Message);

        User user = new() { };
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));
        dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("clientId", dataNotFoundException.Message);
        user.AuthorizingClient = new() { ClientId = ObjectId.GenerateNewId() };
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));
        dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("clientId", dataNotFoundException.Message);

        user.AuthorizingClient = new() { ClientId = clientId, CodeExpiresAt = DateTime.UtcNow.AddMinutes(-2) };
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));
        await Assert.ThrowsAsync<CodeExpirationException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));

        user.AuthorizingClient.CodeExpiresAt = DateTime.UtcNow.AddMinutes(2);
        user.AuthorizingClient.CodeChallengeMethod = "SHA512";
        user.AuthorizingClient.CodeChallenge = "";
        dto.CodeVerifier = "";
        string hashedCodeVerifier = "hashedCodeVerifier";
        string decodedCodeChallenge = "someThingElse";
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(dto.CodeVerifier, user.AuthorizingClient.CodeChallengeMethod)).Returns(hashedCodeVerifier);
        Fixture.IStringHelper.Setup(o => o.Base64Decode(user.AuthorizingClient.CodeChallenge)).Returns(decodedCodeChallenge);
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));
        await Assert.ThrowsAsync<InvalidCodeVerifierException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
    }

    [Fact]
    public async void VerifyAndGenerateTokens_databaseWrite_Failure()
    {
        string code = "code";
        string codeChallenge = "codeChallenge";
        string codeChallengeMethod = "SHA512";
        string codeVerifier = "codeVerifier";
        string hashedCodeVerifier = "hashedCodeVerifier";
        string decodedCodeChallenge = hashedCodeVerifier;
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string token = "token";
        string? hashedToken = "hashedToken";
        TokenPrivileges tokenPrivileges = new();
        ObjectId clientId = ObjectId.GenerateNewId();
        TokenCreateDto dto = new() { ClientId = clientId.ToString(), Code = code, CodeVerifier = codeVerifier, RedirectUrl = Faker.Internet.Url() };
        User user = new()
        {
            AuthorizingClient = new()
            {
                ClientId = clientId,
                TokenPrivileges = tokenPrivileges,
                Code = code,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(TokenManagement.CODE_EXPIRATION_MINUTES),
            }
        };
        DateTime now = DateTime.UtcNow;


        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(dto.CodeVerifier, user.AuthorizingClient.CodeChallengeMethod)).Returns(hashedCodeVerifier);
        Fixture.IStringHelper.Setup(o => o.Base64Decode(user.AuthorizingClient.CodeChallenge)).Returns(decodedCodeChallenge);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl)).Returns(Task.FromResult<Client?>(new() { Id = user.AuthorizingClient.ClientId }));
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));

        Mock<IClientSessionHandle> sessionMock = new Mock<IClientSessionHandle>();
        sessionMock.Setup(o => o.StartTransaction(new(default, default, WriteConcern.WMajority, default)));
        sessionMock.Setup(o => o.AbortTransactionAsync(default));
        sessionMock.Setup(o => o.Dispose());
        IClientSessionHandle session = sessionMock.Object;
        Fixture.IMongoClient.Setup(o => o.StartSessionAsync(null, default)).Returns(Task.FromResult<IClientSessionHandle?>(session));

        Fixture.IUserRepository.Setup(o => o.AddTokenPrivilegesToUser(user.Id, user.Id, clientId, tokenPrivileges, session)).Returns(Task.FromResult<bool?>(null));
        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("user", dataNotFoundException.Message);

        Fixture.IUserRepository.Setup(o => o.AddTokenPrivilegesToUser(user.Id, user.Id, clientId, tokenPrivileges, session)).Returns(Task.FromResult<bool?>(false));
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));

        Fixture.IUserRepository.Setup(o => o.AddTokenPrivilegesToUser(user.Id, user.Id, clientId, tokenPrivileges, session)).Returns(Task.FromResult<bool?>(true));
        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(token);

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns<string?>(null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IStringHelper.SetupSequence(o => o.GenerateRandomString(128)).Returns(token).Returns(refreshToken);

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns<string?>(null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));

        Fixture.IStringHelper.SetupSequence(o => o.GenerateRandomString(128)).Returns(token).Returns(refreshToken);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);

        Fixture.IUserRepository.Setup(o => o.AddAuthorizedClient(user.Id, It.Is<AuthorizedClient>(uc =>
            uc.ClientId.ToString() == clientId.ToString()
            && uc.RefreshToken.TokenPrivileges == tokenPrivileges
            && uc.RefreshToken.Value == hashedRefreshToken
            && uc.RefreshToken.ExpirationDate == now.AddMonths(TokenManagement.REFRESH_TOKEN_EXPIRATION_MONTHS_MONTHS)
            && uc.Token.IsRevoked == false
            && uc.Token.Value == hashedToken
            && uc.Token.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
        ), session)).Returns(Task.FromResult<bool?>(null));
        dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
        Assert.Equal("user", dataNotFoundException.Message);

        Fixture.IStringHelper.SetupSequence(o => o.GenerateRandomString(128)).Returns(token).Returns(refreshToken);
        Fixture.IUserRepository.Setup(o => o.AddAuthorizedClient(user.Id, It.Is<AuthorizedClient>(uc =>
            uc.ClientId.ToString() == clientId.ToString()
            && uc.RefreshToken.TokenPrivileges == tokenPrivileges
            && uc.RefreshToken.Value == hashedRefreshToken
            && uc.RefreshToken.ExpirationDate == now.AddMonths(TokenManagement.REFRESH_TOKEN_EXPIRATION_MONTHS_MONTHS)
            && uc.Token.IsRevoked == false
            && uc.Token.Value == hashedToken
            && uc.Token.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
        ), session)).Returns(Task.FromResult<bool?>(false));
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().VerifyAndGenerateTokens(dto));
    }

    [Fact]
    public async void VerifyAndGenerateTokens()
    {
        string code = "code";
        string codeChallenge = "codeChallenge";
        string codeChallengeMethod = "SHA512";
        string codeVerifier = "codeVerifier";
        string hashedCodeVerifier = "hashedCodeVerifier";
        string decodedCodeChallenge = hashedCodeVerifier;
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string token = "token";
        string? hashedToken = "hashedToken";
        TokenPrivileges tokenPrivileges = new();
        ObjectId clientId = ObjectId.GenerateNewId();
        TokenCreateDto dto = new() { ClientId = clientId.ToString(), Code = code, CodeVerifier = codeVerifier, RedirectUrl = Faker.Internet.Url() };
        User user = new()
        {
            AuthorizingClient = new()
            {
                ClientId = clientId,
                TokenPrivileges = tokenPrivileges,
                Code = code,
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                CodeExpiresAt = DateTime.UtcNow.AddMinutes(TokenManagement.CODE_EXPIRATION_MINUTES),
            }
        };
        DateTime now = DateTime.UtcNow;


        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(dto.CodeVerifier, user.AuthorizingClient.CodeChallengeMethod)).Returns(hashedCodeVerifier);
        Fixture.IStringHelper.Setup(o => o.Base64Decode(user.AuthorizingClient.CodeChallenge)).Returns(decodedCodeChallenge);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl)).Returns(Task.FromResult<Client?>(new() { Id = user.AuthorizingClient.ClientId }));
        Fixture.IUserRepository.Setup(o => o.RetrieveByClientIdAndCode(clientId, dto.Code)).Returns(Task.FromResult<User?>(user));

        Mock<IClientSessionHandle> sessionMock = new Mock<IClientSessionHandle>();
        sessionMock.Setup(o => o.StartTransaction(new(default, default, WriteConcern.WMajority, default)));
        sessionMock.Setup(o => o.AbortTransactionAsync(default));
        sessionMock.Setup(o => o.Dispose());
        IClientSessionHandle session = sessionMock.Object;
        Fixture.IMongoClient.Setup(o => o.StartSessionAsync(null, default)).Returns(Task.FromResult<IClientSessionHandle?>(session));

        Fixture.IUserRepository.Setup(o => o.AddTokenPrivilegesToUser(user.Id, user.Id, clientId, tokenPrivileges, session)).Returns(Task.FromResult<bool?>(true));
        Fixture.IStringHelper.SetupSequence(o => o.GenerateRandomString(128)).Returns(token).Returns(refreshToken);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);

        Fixture.IUserRepository.Setup(o => o.AddAuthorizedClient(user.Id, It.Is<AuthorizedClient>(uc =>
            uc.ClientId.ToString() == clientId.ToString()
            && uc.RefreshToken.TokenPrivileges == tokenPrivileges
            && uc.RefreshToken.Value == hashedRefreshToken
            && uc.RefreshToken.ExpirationDate == now.AddMonths(TokenManagement.REFRESH_TOKEN_EXPIRATION_MONTHS_MONTHS)
            && uc.Token.IsRevoked == false
            && uc.Token.Value == hashedToken
            && uc.Token.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
        ), session)).Returns(Task.FromResult<bool?>(true));
        await InstantiateService().VerifyAndGenerateTokens(dto);
    }

    [Fact]
    public async void ReToken_ClientId_Failure()
    {
        string clientIdStr = "clientIdStr";
        string secret = "secret";
        string refreshToken = "refreshToken";
        await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_secretHash_Failure()
    {
        string clientIdStr = ObjectId.GenerateNewId().ToString();
        string secret = "secret";
        string refreshToken = "refreshToken";
        string? hashedSecret = null;
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_clientValidity_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string secret = "secret";
        string refreshToken = "refreshToken";
        string? hashedSecret = "hashedSecret";
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(null));
        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
        Assert.Equal("client", dataNotFoundException.Message);

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(new() { ExposedCount = 3 }));
        await Assert.ThrowsAsync<BannedClientException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_refreshTokenHash_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string secret = "secret";
        string refreshToken = "refreshToken";
        string? hashedSecret = "hashedSecret";
        string? hashedRefreshToken = null;
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(new()));
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_userRetrieval_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string secret = "secret";
        string refreshToken = "refreshToken";
        string? hashedSecret = "hashedSecret";
        string? hashedRefreshToken = "hashedRefreshToken";
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(new()));
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(null));
        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
        Assert.Equal("user", dataNotFoundException.Message);
    }

    [Fact]
    public async void ReToken_refreshTokenValidity_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string secret = "secret";
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string? hashedToken = "hashedToken";
        string? hashedSecret = "hashedSecret";
        Client? client = new() { Id = clientId };
        User? user = new() { };
        DateTime now = DateTime.UtcNow;

        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(client));
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));

        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
        Assert.Equal("userClient", dataNotFoundException.Message);

        user.AuthorizedClients = new AuthorizedClient[] { new() { ClientId = clientId, RefreshToken = new() { ExpirationDate = now.AddMinutes(-2), Value = hashedRefreshToken }, Token = new() { IsRevoked = false, ExpirationDate = now, Value = hashedToken } } };
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));
        await Assert.ThrowsAsync<ExpiredRefreshTokenException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));

        user.AuthorizedClients[0].RefreshToken!.ExpirationDate = now.AddMinutes(2);

        user.AuthorizedClients[0].RefreshToken!.Value = "anotherRefreshToken";
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_uniqueTokenGeneration_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string token = "token";
        string? hashedToken = "hashedToken";
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string secret = "secret";
        string? hashedSecret = "hashedSecret";
        Client? client = new() { Id = clientId };
        DateTime now = DateTime.UtcNow;
        User? user = new() { AuthorizedClients = new AuthorizedClient[] { new() { ClientId = clientId, RefreshToken = new() { ExpirationDate = now.AddMinutes(2), Value = hashedRefreshToken }, Token = new() { IsRevoked = false, ExpirationDate = now, Value = hashedToken } } } };

        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(client));
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));

        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(token);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns<string?>(null);

        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IUserRepository.Setup(o => o.UpdateToken(user.Id, clientId, It.Is<Token>(t =>
            t.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
            && t.IsRevoked == false
            && t.Value == hashedToken
        ))).Throws<DuplicationException>();
        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken_database_Failure()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string token = "token";
        string? hashedToken = "hashedToken";
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string secret = "secret";
        string? hashedSecret = "hashedSecret";
        Client? client = new() { Id = clientId };
        DateTime now = DateTime.UtcNow;
        User? user = new() { AuthorizedClients = new AuthorizedClient[] { new() { ClientId = clientId, RefreshToken = new() { ExpirationDate = now.AddMinutes(2), Value = hashedRefreshToken }, Token = new() { IsRevoked = false, ExpirationDate = now, Value = hashedToken } } } };

        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(client));
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));

        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(token);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns<string?>(null);

        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IUserRepository.Setup(o => o.UpdateToken(user.Id, clientId, It.Is<Token>(t =>
            t.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
            && t.IsRevoked == false
            && t.Value == hashedToken
        ))).Returns(Task.FromResult<bool?>(null));
        DataNotFoundException dataNotFoundException = await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
        Assert.Equal("client", dataNotFoundException.Message);

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IUserRepository.Setup(o => o.UpdateToken(user.Id, clientId, It.Is<Token>(t =>
            t.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
            && t.IsRevoked == false
            && t.Value == hashedToken
        ))).Returns(Task.FromResult<bool?>(false));
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().ReToken(clientIdStr, secret, refreshToken));
    }

    [Fact]
    public async void ReToken()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string clientIdStr = clientId.ToString();
        string token = "token";
        string? hashedToken = "hashedToken";
        string refreshToken = "refreshToken";
        string? hashedRefreshToken = "hashedRefreshToken";
        string secret = "secret";
        string? hashedSecret = "hashedSecret";
        Client? client = new() { Id = clientId };
        DateTime now = DateTime.UtcNow;
        User? user = new() { AuthorizedClients = new AuthorizedClient[] { new() { ClientId = clientId, RefreshToken = new() { ExpirationDate = now.AddMinutes(2), Value = hashedRefreshToken }, Token = new() { IsRevoked = false, ExpirationDate = now, Value = hashedToken } } } };

        Fixture.IDateTimeProvider.Setup(o => o.ProvideUtcNow()).Returns(now);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(refreshToken, "SHA512")).Returns(hashedRefreshToken);
        Fixture.IClientRepository.Setup(o => o.RetrieveByIdAndSecret(clientId, hashedSecret)).Returns(Task.FromResult<Client?>(client));
        Fixture.IUserRepository.Setup(o => o.RetrieveByRefreshTokenValue(hashedRefreshToken)).Returns(Task.FromResult<User?>(user));

        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(128)).Returns(token);
        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(token, "SHA512")).Returns(hashedToken);
        Fixture.IUserRepository.Setup(o => o.UpdateToken(user.Id, clientId, It.Is<Token>(t =>
            t.ExpirationDate == now.AddMonths(TokenManagement.TOKEN_EXPIRATION_MONTHS)
            && t.IsRevoked == false
            && t.Value == hashedToken
        ))).Returns(Task.FromResult<bool?>(true));

        string result = await InstantiateService().ReToken(clientIdStr, secret, refreshToken);
        Assert.Equal(token, result);
    }
}
