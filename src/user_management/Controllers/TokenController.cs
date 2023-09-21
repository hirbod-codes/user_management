namespace user_management.Controllers;

using System.Security.Authentication;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using user_management.Authentication;
using user_management.Authorization.Attributes;
using user_management.Controllers.Services;
using user_management.Data;
using user_management.Dtos.Token;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;
using user_management.Services.Data.Client;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class TokenController : ControllerBase
{
    private readonly ITokenManagement _tokenManagement;
    private readonly IMapper _mapper;
    private readonly IAuthenticatedByJwt _authenticatedByJwt;

    public TokenController(ITokenManagement tokenManagement, IMapper mapper, IAuthenticatedByJwt authenticatedByJwt)
    {
        _tokenManagement = tokenManagement;
        _mapper = mapper;
        _authenticatedByJwt = authenticatedByJwt;
    }

    [Permissions(Permissions = new string[] { StaticData.AUTHORIZE_CLIENT })]
    [HttpPost(PATH_POST_AUTHORIZE)]
    public async Task<IActionResult> Authorize(TokenAuthDto tokenAuthDto)
    {
        if (!_authenticatedByJwt.IsAuthenticated()) return Unauthorized();

        if (tokenAuthDto.ResponseType != "code") return BadRequest("Unsupported response type requested.");

        string r = null!;
        try
        {
            r = await _tokenManagement.Authorize(
                tokenAuthDto.ClientId,
                tokenAuthDto.RedirectUrl,
                tokenAuthDto.CodeChallenge,
                tokenAuthDto.CodeChallengeMethod,
                _mapper.Map<TokenPrivileges>(tokenAuthDto.Scope)
            );
        }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (BannedClientException) { return NotFound("System failed to find the client."); }
        catch (DataNotFoundException) { return NotFound("System failed to find the client."); }
        catch (ArgumentException) { return BadRequest("Invalid client id provided."); }
        catch (UnauthorizedAccessException) { return StatusCode(403); }
        catch (DuplicationException) { return Problem("Internal server error encountered."); }
        catch (DatabaseServerException) { return Problem("Internal server error encountered."); }

        return RedirectPermanent(tokenAuthDto.RedirectUrl + $"?code={r}&state={tokenAuthDto.State}");
    }

    [HttpPost(PATH_POST_TOKEN_VERIFICATION)]
    public async Task<IActionResult> VerifyAndGenerateTokens(TokenCreateDto tokenCreateDto)
    {
        if (tokenCreateDto.GrantType != "authorization_code") return BadRequest("only 'authorization_code' grant type is supported");

        try { return Ok(await _tokenManagement.VerifyAndGenerateTokens(tokenCreateDto)); }
        catch (BannedClientException) { return NotFound("System failed to find the client."); }
        catch (CodeExpirationException) { return BadRequest("The code is expired, please redirect user again for another authorization."); }
        catch (InvalidCodeVerifierException) { return BadRequest("The code verifier is invalid."); }
        catch (DataNotFoundException ex)
        {
            switch (ex.Message)
            {
                case "client":
                    return NotFound("We don't have a client with this client id: " + tokenCreateDto.ClientId + " and this redirect url: " + tokenCreateDto.RedirectUrl);
                case "user":
                    return NotFound("We couldn't find your account.");
                case "clientId":
                    return NotFound("You haven't authorized a client with the provided client id.");
                case "refreshToken":
                    return NotFound("We couldn't find your refresh token.");
                default:
                    return Problem();
            }
        }
        catch (DuplicationException) { return Problem("We couldn't generate a token for you."); }
        catch (DatabaseServerException) { return Problem("Internal server error encountered"); }
        catch (OperationException) { return Problem("Internal server error encountered"); }
    }

    [HttpPost(PATH_POST_RETOKEN)]
    public async Task<IActionResult> ReToken(ReTokenDto reTokenDto)
    {
        try { return Ok(await _tokenManagement.ReToken(reTokenDto.ClientId, reTokenDto.ClientSecret, reTokenDto.RefreshToken)); }
        catch (OperationException) { return Problem(); }
        catch (BannedClientException) { return NotFound("System failed to find the client."); }
        catch (InvalidRefreshTokenException) { return BadRequest("The refresh token is invalid."); }
        catch (ExpiredRefreshTokenException) { return BadRequest("The refresh token is expired."); }
        catch (DataNotFoundException) { return NotFound("There is no such refresh token."); }
        catch (DatabaseServerException) { return Problem("Internal server error encountered."); }
        catch (DuplicationException) { return Problem("Internal server error encountered."); }
    }

    public const string PATH_POST_AUTHORIZE = "auth";
    public const string PATH_POST_TOKEN_VERIFICATION = "token";
    public const string PATH_POST_RETOKEN = "retoken";
}
