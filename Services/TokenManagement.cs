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
    public const int REFRESH_TOKEN_EXPIRATION_MONTHS = 2;
    public const int CODE_EXPIRATION_MINUTES = 3;
    public const int TOKEN_EXPIRATION = 1;
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
        if (user.Clients.FirstOrDefault(c => c != null && c.ClientId.ToString() == clientId) != null) await _userRepository.RemoveClient(user.Id, clientObjectId, user.Id, false);

        Models.Client? client = await _clientRepository.RetrieveByIdAndRedirectUrl(clientObjectId, redirectUrl);
        if (client == null) throw new DataNotFoundException("client");

        if (client.ExposedCount > 2) throw new BannedClientException();

        if (!StaticData.AreValid(scope.Privileges, user.Privileges!)) throw new UnauthorizedAccessException();

        UserClient userClient = new UserClient()
        {
            ClientId = clientObjectId,
            RefreshToken = new RefreshToken()
            {
                TokenPrivileges = scope,
                Code = null,
                CodeExpiresAt = _dateTimeProvider.ProvideUtcNow().AddMinutes(CODE_EXPIRATION_MINUTES),
                CodeChallenge = codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddMonths(REFRESH_TOKEN_EXPIRATION_MONTHS),
                Value = _stringHelper.HashWithoutSalt(_stringHelper.GenerateRandomString(128))!,
                IsVerified = false
            },
            Token = null
        };

        int safety = 0;
        do
        {
            userClient.RefreshToken.Code = _stringHelper.GenerateRandomString(128);

            bool? r = null;
            try { r = await _userRepository.AddClientById(user.Id, user.Id, userClient); }
            catch (DuplicationException) { safety++; continue; }

            if (r == true) break;
            else if (r == false) throw new DatabaseServerException();
            else throw new DataNotFoundException();
        } while (safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return userClient.RefreshToken!.Code!;
    }

    public async Task<(string token, string refreshToken)> Token(string clientId, TokenCreateDto dto)
    {
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException();

        Models.Client? client = await _clientRepository.RetrieveByIdAndRedirectUrl(clientObjectId, dto.RedirectUrl);
        if (client == null) throw new DataNotFoundException("client");
        if (client.ExposedCount > 2) throw new BannedClientException();

        User? user = await _userRepository.RetrieveByClientIdAndCode(client.Id, dto.Code);
        if (user == null) throw new DataNotFoundException("user");

        UserClient? userClient = user.Clients.ToList().FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == client.Id, null);
        if (userClient == null) throw new DataNotFoundException("clientId");

        RefreshToken? refreshToken = userClient.RefreshToken;
        if (refreshToken == null)
            throw new DataNotFoundException("refreshToken");
        if (refreshToken.ExpirationDate < _dateTimeProvider.ProvideUtcNow())
            throw new RefreshTokenExpirationException();
        if (refreshToken.CodeExpiresAt == null || refreshToken.CodeExpiresAt < _dateTimeProvider.ProvideUtcNow())
            throw new CodeExpirationException();
        if (_stringHelper.HashWithoutSalt(dto.CodeVerifier, refreshToken.CodeChallengeMethod) != _stringHelper.Base64Decode(refreshToken.CodeChallenge))
            throw new InvalidCodeVerifierException();

        string tokenValue = null!;
        using (IClientSessionHandle session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction(new(writeConcern: WriteConcern.WMajority));

            bool? refreshTokenVerificationResult = await _userRepository.VerifyRefreshToken(user.Id, client.Id, session);
            if (refreshTokenVerificationResult == null)
            {
                await session.AbortTransactionAsync();
                throw new DataNotFoundException("user");
            }
            if (refreshTokenVerificationResult == false)
            {
                await session.AbortTransactionAsync();
                throw new DatabaseServerException();
            }

            bool? userResult = await _userRepository.AddTokenPrivilegesToUser(user.Id, user.Id, client.Id, refreshToken.TokenPrivileges, session);
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

            Models.Token token = new() { Value = null, ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddHours(TOKEN_EXPIRATION), IsRevoked = false };
            int safety = 0;
            do
            {
                tokenValue = _stringHelper.GenerateRandomString(128);
                token.Value = _stringHelper.HashWithoutSalt(tokenValue);
                if (token.Value == null) throw new OperationException();

                bool? addTokenResult = null;

                try { addTokenResult = await _userRepository.AddToken(user.Id, user.Id, client.Id, token, session); }
                catch (DuplicationException) { safety++; continue; }

                if (addTokenResult == true) break;

                if (addTokenResult == null)
                {
                    await session.AbortTransactionAsync();
                    throw new DataNotFoundException("user");
                }
                if (addTokenResult == false)
                {
                    await session.AbortTransactionAsync();
                    throw new DatabaseServerException();
                }
                safety++;
            } while (safety < 200);

            if (safety >= 200)
            {
                await session.AbortTransactionAsync();
                throw new DuplicationException();
            }

            await session.CommitTransactionAsync();
        }

        return (tokenValue, refreshToken.Value);
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

        List<UserClient> userClients = user.Clients.ToList();
        UserClient? userClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null && uc.ClientId == client.Id, null);
        if (userClient == null) throw new DataNotFoundException("userClient");
        if (userClient.RefreshToken == null) throw new InvalidRefreshTokenException();
        if (userClient.RefreshToken.ExpirationDate < _dateTimeProvider.ProvideUtcNow()) throw new ExpiredRefreshTokenException();
        if (!userClient.RefreshToken.IsVerified) throw new UnverifiedRefreshTokenException();
        if (userClient.RefreshToken.Value != hashedRefreshToken) throw new InvalidRefreshTokenException();

        Token token = new() { ExpirationDate = _dateTimeProvider.ProvideUtcNow().AddHours(TOKEN_EXPIRATION), Value = null, IsRevoked = false };
        string tokenValue = null!;

        int safety = 0;
        do
        {
            tokenValue = _stringHelper.GenerateRandomString(128);
            token.Value = _stringHelper.HashWithoutSalt(tokenValue);

            bool? r = null;
            try { r = await _userRepository.AddToken(user.Id, user.Id, clientObjectId, token); }
            catch (DuplicationException) { safety++; continue; }

            if (r == true) break;
            if (r == null) throw new DataNotFoundException("client");
            if (r == false) throw new DatabaseServerException();
            safety++;
        } while (safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return tokenValue;
    }
}