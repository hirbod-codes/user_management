namespace user_management.Authentication.JWT;

using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using user_management.Authentication;
using user_management.Models;
using user_management.Services.Data.User;

public class JWTAuthenticationHandler : AuthenticationHandler<JWTAuthenticationOptions>
{
    private JWTAuthenticationOptions _options;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticatedByJwt _authenticatedByJwt;
    private readonly IAuthenticated _authenticated;

    public string? Error { get; private set; }

    public JWTAuthenticationHandler(IUserRepository userRepository, IOptionsMonitor<JWTAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IAuthenticatedByJwt authenticatedByJwt, IAuthenticated authenticated) : base(options, logger, encoder, clock)
    {
        _options = options.CurrentValue;
        _userRepository = userRepository;
        _authenticatedByJwt = authenticatedByJwt;
        _authenticated = authenticated;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var authenticationHeaderValue)) return AuthenticateResult.NoResult();

        string[] authenticationHeaderParts = authenticationHeaderValue.ToString().Split(" ");
        if (authenticationHeaderParts.Length != 2)
            return AuthenticateResult.Fail("No JWT token provided.");

        string token = authenticationHeaderParts[1];

        if (authenticationHeaderParts[0] != "Bearer") return AuthenticateResult.Fail("No JWT token provided.");

        if (!ValidateJwt(token, out SecurityToken validatedToken)) return AuthenticateResult.Fail(Error!);

        List<Claim> claims = GetTokenClaims(token).ToList();

        string userId;
        try { userId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value; }
        catch (ArgumentNullException) { return AuthenticateResult.Fail("Subject is not provided."); }
        catch (InvalidOperationException) { return AuthenticateResult.Fail("Invalid subject provided."); }

        User? user = await _userRepository.RetrieveByIdForAuthenticationHandling(userId);
        if (user == null) return AuthenticateResult.Fail("This JWT token is no longer valid.");

        _authenticatedByJwt.SetAuthenticated(user);
        _authenticated.SetAuthenticationType("JWT");
        _authenticated.SetAuthenticatedIdentifier(user.Id.ToString());
        _authenticated.SetIdentifierToken(token);

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);

        ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(identity);

        AuthenticationTicket ticket = new AuthenticationTicket(claimPrincipal, authenticationScheme: "JWT");

        return AuthenticateResult.Success(ticket);
    }

    public IEnumerable<Claim> GetTokenClaims(string token) => GetClaimsPrincipal(token, out SecurityToken validatedToken).Claims;

    public bool ValidateJwt(string token, out SecurityToken validatedToken)
    {
        SecurityToken securityToken = null!;
        try
        {
            GetClaimsPrincipal(token, out securityToken);
        }
        catch (ArgumentNullException) { Error = "No authorization token provided."; }
        catch (ArgumentException) { Error = "Invalid token provided."; }
        catch (SecurityTokenEncryptionKeyNotFoundException) { Error = "Internal server error encountered."; }
        catch (SecurityTokenDecryptionFailedException) { Error = "Corrupted token provided."; }
        catch (SecurityTokenExpiredException) { Error = "The provided token has been expired."; }
        catch (SecurityTokenInvalidAudienceException) { Error = "Invalid Audience."; }
        catch (SecurityTokenInvalidLifetimeException) { Error = "The provided token has been expired."; }
        catch (SecurityTokenInvalidSignatureException) { Error = "The Provided token is modified."; }
        catch (SecurityTokenNoExpirationException) { Error = "The provided token has no expiration time."; }
        catch (SecurityTokenNotYetValidException) { Error = "Invalid token provided."; }
        catch (SecurityTokenReplayAddFailedException) { Error = "Invalid token provided."; }
        catch (SecurityTokenReplayDetectedException) { Error = "Invalid token provided."; }
        catch (SecurityTokenException) { Error = "Invalid token provided."; }

        validatedToken = securityToken;
        return Error == null || Error == "";
    }

    private ClaimsPrincipal GetClaimsPrincipal(string token, out SecurityToken validatedToken)
    {
        ClaimsPrincipal claimsPrincipal = (new JwtSecurityTokenHandler()).ValidateToken(token, GetTokenValidationParameters(), out SecurityToken securityToken);
        validatedToken = securityToken;
        return claimsPrincipal;
    }

    private TokenValidationParameters GetTokenValidationParameters() => new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidAudience = _options.Audience,
        ValidIssuer = _options.Issuer,
        IssuerSigningKey = GetSymmetricSecurityKey()
    };

    private SecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Convert.FromBase64String(_options.SecretKey));
}
