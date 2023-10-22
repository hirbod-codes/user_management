using user_management.Utilities;
using user_management.Services.Client;
using user_management.Controllers.Services;
using user_management.Services.Data.User;
using MongoDB.Bson;
using user_management.Dtos.Token;
using user_management.Models;
using user_management.Services.Data;
using MongoDB.Driver;
using user_management.Services.Data.Client;
using user_management.Authentication;
using user_management.Data;

namespace user_management.Services;

public class TokenManagement : ITokenManagement
{
    public const int REFRESH_TOKEN_EXPIRATION_MONTHS_MONTHS = 2;
    public const int CODE_EXPIRATION_MINUTES = 3;
    public const int TOKEN_EXPIRATION_MONTHS = 1;
    private readonly IAuthenticatedByJwt _authenticatedByJwt;
    private readonly IClientRepository _clientRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMongoClient _mongoClient;
    private readonly IStringHelper _stringHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenManagement(IStringHelper stringHelper, IClientRepository clientRepository, IUserRepository userRepository, IDateTimeProvider dateTimeProvider, IMongoClient mongoClient, IAuthenticatedByJwt authenticatedByJwt)
    {
        _stringHelper = stringHelper;
        _clientRepository = clientRepository;
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
        _mongoClient = mongoClient;
        _authenticatedByJwt = authenticatedByJwt;
    }

    public async Task<string> Authorize(
        string clientId,
        string redirectUrl,
        string codeChallenge,
        string codeChallengeMethod,
        TokenPrivileges scope
        )
    {
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException("clientId");

        User user = await _authenticatedByJwt.GetAuthenticated();
        if (user.AuthorizedClients.FirstOrDefault(c => c != null && c.ClientId.ToString() == clientId) != null) await _userRepository.RemoveClient(user.Id, clientObjectId, user.Id, false);

        Models.Client? client = await _clientRepository.RetrieveByIdAndRedirectUrl(clientObjectId, redirectUrl);
        if (client == null) throw new DataNotFoundException("client");

        if (client.ExposedCount > 2) throw new BannedClientException();

        if (!StaticData.AreValid(scope.Privileges, user.Privileges!)) throw new UnauthorizedAccessException();

        AuthorizingClient authorizingClient = new()
        {
            ClientId = clientObjectId,
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = codeChallengeMethod,
            TokenPrivileges = scope
        };

        int safety = 0;
        do
        {
            authorizingClient.Code = _stringHelper.GenerateRandomString(128);
            authorizingClient.CodeExpiresAt = _dateTimeProvider.ProvideUtcNow().AddMinutes(CODE_EXPIRATION_MINUTES);

            bool? r = null;
            try { r = await _userRepository.UpdateAuthorizingClient(user.Id, authorizingClient); }
            catch (DuplicationException) { safety++; continue; }

            if (r == true) break;
            else if (r == false) throw new DatabaseServerException();
            else throw new DataNotFoundException();
        } while (safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return authorizingClient.Code;
    }

    public async Task<TokenRetrieveDto> VerifyAndGenerateTokens(TokenCreateDto dto)
    {
        if (!ObjectId.TryParse(dto.ClientId, out ObjectId clientId)) throw new ArgumentException();

        Models.Client? client = await _clientRepository.RetrieveByIdAndRedirectUrl(clientId, dto.RedirectUrl);
        if (client == null) throw new DataNotFoundException("client");
        if (client.ExposedCount > 2) throw new BannedClientException();

        User? user = await _userRepository.RetrieveByClientIdAndCode(client.Id, dto.Code);
        if (user == null) throw new DataNotFoundException("user");

        if (user.AuthorizingClient == null || user.AuthorizingClient.ClientId.ToString() != clientId.ToString())
            throw new DataNotFoundException("clientId");
        if (user.AuthorizingClient.CodeExpiresAt < _dateTimeProvider.ProvideUtcNow())
            throw new CodeExpirationException();
        if (_stringHelper.HashWithoutSalt(dto.CodeVerifier, user.AuthorizingClient.CodeChallengeMethod) != _stringHelper.Base64Decode(user.AuthorizingClient.CodeChallenge))
            throw new InvalidCodeVerifierException();

        string tokenValue = null!;
        string refreshToken = null!;
        using (IClientSessionHandle session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction(new(writeConcern: WriteConcern.WMajority));

            bool? userResult = await _userRepository.AddTokenPrivilegesToUser(user.Id, user.Id, client.Id, user.AuthorizingClient.TokenPrivileges, session);
            if (userResult == null)
            {
                await session.AbortTransactionAsync();
                throw new DataNotFoundException("user");
            }
            if (userResult == false)
            {
                await session.AbortTransactionAsync();
                throw new DatabaseServerException();
            }

            AuthorizedClient authorizedClient = new()
            {
                ClientId = clientId,
                RefreshToken = new()
                {
                    TokenPrivileges = user.AuthorizingClient.TokenPrivileges,
                },
                Token = new()
                {
                    IsRevoked = false
                }
            };

            bool? authorizedClientResult;
            int safety = 0;
            do
            {
                tokenValue = _stringHelper.GenerateRandomString(128);
                string? hashedTokenValue = _stringHelper.HashWithoutSalt(tokenValue);
                if (hashedTokenValue == null) throw new OperationException();

                refreshToken = _stringHelper.GenerateRandomString(128);
                string? hashedRefreshToken = _stringHelper.HashWithoutSalt(refreshToken);
                if (hashedRefreshToken == null) throw new OperationException();

                authorizedClient.RefreshToken.Value = hashedRefreshToken;
                authorizedClient.RefreshToken.ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddMonths(REFRESH_TOKEN_EXPIRATION_MONTHS_MONTHS);
                authorizedClient.Token.Value = hashedTokenValue;
                authorizedClient.Token.ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddMonths(TOKEN_EXPIRATION_MONTHS);

                try { authorizedClientResult = await _userRepository.AddAuthorizedClient(user.Id, authorizedClient, session); }
                catch (DuplicationException) { safety++; continue; }

                if (authorizedClientResult == true) break;

                if (authorizedClientResult == null)
                {
                    await session.AbortTransactionAsync();
                    throw new DataNotFoundException("user");
                }

                if (authorizedClientResult == false)
                {
                    await session.AbortTransactionAsync();
                    throw new DatabaseServerException();
                }
            } while (safety < 200);

            if (safety >= 200)
            {
                await session.AbortTransactionAsync();
                throw new DuplicationException();
            }

            await session.CommitTransactionAsync();
        }

        return new() { RefreshToken = refreshToken, Token = tokenValue };
    }

    public async Task<string> ReToken(string clientId, string secret, string refreshToken)
    {
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException();

        string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
        if (hashedSecret == null) throw new OperationException();

        Models.Client? client = await _clientRepository.RetrieveByIdAndSecret(clientObjectId, hashedSecret);
        if (client == null) throw new DataNotFoundException("client");
        if (client.ExposedCount > 2) throw new BannedClientException();

        string? hashedRefreshToken = _stringHelper.HashWithoutSalt(refreshToken);
        if (hashedRefreshToken == null) throw new OperationException();
        User? user = await _userRepository.RetrieveByRefreshTokenValue(hashedRefreshToken);
        if (user == null) throw new DataNotFoundException("user");

        List<AuthorizedClient> userClients = user.AuthorizedClients.ToList();
        AuthorizedClient? userClient = userClients.FirstOrDefault<AuthorizedClient?>(uc => uc != null && uc.ClientId == client.Id, null);
        if (userClient == null) throw new DataNotFoundException("userClient");
        if (userClient.RefreshToken.ExpirationDate < _dateTimeProvider.ProvideUtcNow()) throw new ExpiredRefreshTokenException();
        if (userClient.RefreshToken.Value != hashedRefreshToken) throw new InvalidRefreshTokenException();

        string tokenValue = null!;

        int safety = 0;
        do
        {
            tokenValue = _stringHelper.GenerateRandomString(128);
            string? hashedTokenValue = _stringHelper.HashWithoutSalt(tokenValue);
            if (hashedTokenValue == null) throw new OperationException();

            Token token = new() { ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddMonths(TOKEN_EXPIRATION_MONTHS), Value = hashedTokenValue, IsRevoked = false };

            bool? r = null;
            try { r = await _userRepository.UpdateToken(user.Id, clientObjectId, token); }
            catch (DuplicationException) { safety++; continue; }

            if (r == true) break;
            if (r == null) throw new DataNotFoundException("client");
            if (r == false) throw new DatabaseServerException();
        } while (safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return tokenValue;
    }
}
