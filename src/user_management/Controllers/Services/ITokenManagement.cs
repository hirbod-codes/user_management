using MongoDB.Bson;
using user_management.Dtos.Token;
using user_management.Models;

namespace user_management.Controllers.Services;

public interface ITokenManagement
{
    /// <summary>
    /// Creates a UserClient object with an unverified refreshToken field and stores it in user document in database.<br/>
    /// 
    /// Checks:<br/>
    /// 1.check if clientId is valid<br/>
    /// 2.check authentication and the existence of the UserClient in authenticated user<br/>
    /// 3.check client exists and is not exposed more than 2 times<br/>
    /// 4.check if authenticated user has requested privileges by the token<br/>
    /// 
    /// Delete UserCLient object if it already exists.<br/>
    /// Create UserCLient object<br/>
    /// Try to add the UserClient object authenticated user in db with a unique code<br/>
    /// return code if successful<br/>
    /// throw DuplicationException if failed<br/>
    /// </summary>
    /// <returns>Generated code for the third party client.</returns>
    /// <exception cref="user_management.Services.Data.Client.BannedClientException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="System.UnauthorizedAccessException"></exception>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    public Task<string> Authorize(
        string clientId,
        string redirectUrl,
        string codeChallenge,
        string codeChallengeMethod,
        TokenPrivileges scope
    );

    /// <summary>
    /// This method must be called for the first token generation.
    /// Checks:
    ///     refresh token code.
    ///     refresh token expiration date.
    ///     client existence.
    ///     client exposure.
    /// Then verifies the refreshToken field.
    /// </summary>
    /// <returns>The generated token for refreshToken and token fields.</returns>
    /// <exception cref="user_management.Services.Data.Client.BannedClientException"></exception>
    /// <exception cref="user_management.Services.Client.CodeExpirationException"></exception>
    /// <exception cref="user_management.Services.Client.InvalidCodeVerifierException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    public Task<TokenRetrieveDto> VerifyAndGenerateTokens(TokenCreateDto dto);

    /// <summary>
    /// This method must be called for the first token generation.
    /// Checks:
    ///     refresh token verification.
    ///     refresh token code.
    ///     refresh token expiration date.
    ///     client existence.
    ///     client exposure.
    /// </summary>
    /// <returns>The generated token.</returns>
    /// <exception cref="System.ArgumentException"></exception>
    /// <exception cref="user_management.Services.OperationException"></exception>
    /// <exception cref="user_management.Services.Data.Client.BannedClientException"></exception>
    /// <exception cref="user_management.Services.Data.Client.InvalidRefreshTokenException"></exception>
    /// <exception cref="user_management.Services.Data.Client.ExpiredRefreshTokenException"></exception>
    /// <exception cref="user_management.Services.Data.DataNotFoundException"></exception>
    /// <exception cref="user_management.Services.Data.DatabaseServerException"></exception>
    /// <exception cref="user_management.Services.Data.DuplicationException"></exception>
    public Task<string> ReToken(string clientId, string secret, string refreshToken);
}
