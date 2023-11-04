using System.Security.Authentication;
using Bogus;
using MongoDB.Bson;
using user_management.Dtos.Token;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;
using user_management.Services.Data.Client;

namespace user_management_unit_tests.Controllers;

[Collection("Controller")]
public class TokenControllerTests
{
    public ControllerFixture Fixture { get; private set; }

    public TokenControllerTests(ControllerFixture controllerFixture) => Fixture = controllerFixture;

    private user_management.Controllers.V1.TokenController InstantiateController() => new user_management.Controllers.V1.TokenController(Fixture.ITokenManagement.Object, Fixture.IMapper.Object, Fixture.IAuthenticatedByJwt.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void Authorize_Unauthenticated()
    {
        TokenAuthDto dto = new() { ResponseType = "not code" };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Authorize(dto));

        dto.ResponseType = "code";
        TokenPrivileges scope = new();
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().Authorize(dto));
    }

    [Fact]
    public async void Authorize_Unauthorized()
    {
        TokenAuthDto dto = new() { ResponseType = "code" };
        TokenPrivileges scope = new();
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(true);
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<UnauthorizedAccessException>();
        HttpAsserts.IsUnauthorized(await InstantiateController().Authorize(dto));
    }

    [Fact]
    public async void Authorize_BadRequest()
    {
        TokenAuthDto dto = new()
        {
            ClientId = ObjectId.GenerateNewId().ToString(),
            ResponseType = "not code",
            CodeChallenge = "CodeChallenge",
            CodeChallengeMethod = "CodeChallengeMethod",
            RedirectUrl = Faker.Internet.Url(),
            Scope = new(),
            State = "State"
        };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(true);
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Authorize(dto), "Unsupported response type requested.");

        dto.ResponseType = "code";
        TokenPrivileges scope = new();
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<ArgumentException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Authorize(dto), "Invalid client id provided.");
    }

    [Fact]
    public async void Authorize_NotFound()
    {
        TokenAuthDto dto = new()
        {
            ClientId = ObjectId.GenerateNewId().ToString(),
            ResponseType = "code",
            CodeChallenge = "CodeChallenge",
            CodeChallengeMethod = "CodeChallengeMethod",
            RedirectUrl = Faker.Internet.Url(),
            Scope = new(),
            State = "State"
        };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(true);
        TokenPrivileges scope = new();
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<BannedClientException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().Authorize(dto), "System failed to find the client.");

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().Authorize(dto), "System failed to find the client.");
    }

    [Fact]
    public async void Authorize_Problem()
    {
        TokenAuthDto dto = new()
        {
            ClientId = ObjectId.GenerateNewId().ToString(),
            ResponseType = "code",
            CodeChallenge = "CodeChallenge",
            CodeChallengeMethod = "CodeChallengeMethod",
            RedirectUrl = Faker.Internet.Url(),
            Scope = new(),
            State = "State"
        };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(true);
        TokenPrivileges scope = new();
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().Authorize(dto), "Internal server error encountered.");

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().Authorize(dto), "Internal server error encountered.");
    }

    [Fact]
    public async void Authorize_Ok()
    {
        TokenAuthDto dto = new()
        {
            ClientId = ObjectId.GenerateNewId().ToString(),
            ResponseType = "code",
            CodeChallenge = "CodeChallenge",
            CodeChallengeMethod = "CodeChallengeMethod",
            RedirectUrl = Faker.Internet.Url(),
            Scope = new(),
            State = "State"
        };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(true);
        TokenPrivileges scope = new();
        Fixture.IMapper.Setup(o => o.Map<TokenPrivileges>(dto.Scope)).Returns(scope);
        string code = "code";

        Fixture.ITokenManagement.Setup(o => o.Authorize(
                dto.ClientId,
                dto.RedirectUrl,
                dto.CodeChallenge,
                dto.CodeChallengeMethod,
                scope
            )).Returns(Task.FromResult<string>(code));
        HttpAsserts.IsRedirectResult(await InstantiateController().Authorize(dto), dto.RedirectUrl + $"?code={code}&state={dto.State}");
    }

    [Fact]
    public async void ReToken_BadRequest()
    {
        ReTokenDto dto = new() { ClientId = ObjectId.GenerateNewId().ToString(), ClientSecret = Faker.Random.String2(128), RefreshToken = Faker.Random.String2(128) };

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<InvalidRefreshTokenException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().ReToken(dto), "The refresh token is invalid.");

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<ExpiredRefreshTokenException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().ReToken(dto), "The refresh token is expired.");
    }

    [Fact]
    public async void VerifyAndGenerateTokens_BadRequest()
    {
        TokenCreateDto dto = new() { GrantType = "Not authorization_code" };

        HttpAsserts<string>.IsBadRequest(await InstantiateController().VerifyAndGenerateTokens(dto), "only 'authorization_code' grant type is supported");

        dto.GrantType = "authorization_code";

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<CodeExpirationException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().VerifyAndGenerateTokens(dto), "The code is expired, please redirect user again for another authorization.");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<InvalidCodeVerifierException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().VerifyAndGenerateTokens(dto), "The code verifier is invalid.");
    }

    [Fact]
    public async void VerifyAndGenerateTokens_NotFound()
    {
        TokenCreateDto dto = new() { GrantType = "authorization_code", ClientId = ObjectId.GenerateNewId().ToString(), RedirectUrl = Faker.Internet.Url() };

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<BannedClientException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().VerifyAndGenerateTokens(dto), "System failed to find the client.");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws(new DataNotFoundException("client"));
        HttpAsserts<string>.IsNotFound(await InstantiateController().VerifyAndGenerateTokens(dto), "We don't have a client with this client id: " + dto.ClientId + " and this redirect url: " + dto.RedirectUrl);

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws(new DataNotFoundException("user"));
        HttpAsserts<string>.IsNotFound(await InstantiateController().VerifyAndGenerateTokens(dto), "We couldn't find your account.");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws(new DataNotFoundException("clientId"));
        HttpAsserts<string>.IsNotFound(await InstantiateController().VerifyAndGenerateTokens(dto), "You haven't authorized a client with the provided client id.");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws(new DataNotFoundException("refreshToken"));
        HttpAsserts<string>.IsNotFound(await InstantiateController().VerifyAndGenerateTokens(dto), "We couldn't find your refresh token.");
    }

    [Fact]
    public async void VerifyAndGenerateTokens_Problem()
    {
        TokenCreateDto dto = new() { GrantType = "authorization_code", ClientId = ObjectId.GenerateNewId().ToString(), RedirectUrl = Faker.Internet.Url() };

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<DataNotFoundException>();
        HttpAsserts.IsProblem(await InstantiateController().VerifyAndGenerateTokens(dto));

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().VerifyAndGenerateTokens(dto), "We couldn't generate a token for you.");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().VerifyAndGenerateTokens(dto), "Internal server error encountered");

        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().VerifyAndGenerateTokens(dto), "Internal server error encountered");
    }

    [Fact]
    public async void VerifyAndGenerateTokens_Ok()
    {
        TokenCreateDto dto = new() { GrantType = "authorization_code", ClientId = ObjectId.GenerateNewId().ToString(), RedirectUrl = Faker.Internet.Url() };

        TokenRetrieveDto result = new() { Token = "token", RefreshToken = "refreshToken" };
        Fixture.ITokenManagement.Setup(o => o.VerifyAndGenerateTokens(dto)).Returns(Task.FromResult(result));
        HttpAsserts<object>.IsOk(await InstantiateController().VerifyAndGenerateTokens(dto), result);
    }

    [Fact]
    public async void ReToken_Problem()
    {
        ReTokenDto dto = new() { ClientId = ObjectId.GenerateNewId().ToString(), ClientSecret = Faker.Random.String2(128), RefreshToken = Faker.Random.String2(128) };

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().ReToken(dto));

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().ReToken(dto), "Internal server error encountered.");

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().ReToken(dto), "Internal server error encountered.");
    }

    [Fact]
    public async void ReToken_NotFound()
    {
        ReTokenDto dto = new() { ClientId = ObjectId.GenerateNewId().ToString(), ClientSecret = Faker.Random.String2(128), RefreshToken = Faker.Random.String2(128) };

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<BannedClientException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().ReToken(dto), "System failed to find the client.");

        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().ReToken(dto), "There is no such refresh token.");
    }

    [Fact]
    public async void ReToken_Ok()
    {
        ReTokenDto dto = new() { ClientId = ObjectId.GenerateNewId().ToString(), ClientSecret = Faker.Random.String2(128), RefreshToken = Faker.Random.String2(128) };
        string tokenValue = "tokenValue";
        Fixture.ITokenManagement.Setup(o => o.ReToken(dto.ClientId, dto.ClientSecret, dto.RefreshToken)).Returns(Task.FromResult(tokenValue));
        HttpAsserts<string>.IsOk(await InstantiateController().ReToken(dto), tokenValue);
    }
}
