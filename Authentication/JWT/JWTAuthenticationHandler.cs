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

public class JWTAuthenticationHandler : AuthenticationHandler<JWTAuthenticationOptions>, IJWTAuthenticationHandler
{
    private JWTAuthenticationOptions _options;

    public string? Error { get; private set; }

    public JWTAuthenticationHandler(IOptionsMonitor<JWTAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _options = options.CurrentValue;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var authenticationHeaderValue)) return Task.FromResult<AuthenticateResult>(AuthenticateResult.NoResult());

        string[] authenticationHeaderParts = authenticationHeaderValue.ToString().Split(" ");
        string token = authenticationHeaderParts[1];

        if (authenticationHeaderParts[0] != "JWT") return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("No JWT token provided."));

        if (!ValidateJwt(token, out SecurityToken validatedToken)) return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail(Error!));

        List<Claim> claims = GetTokenClaims(token).ToList();

        string userId;
        try { userId = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value; }
        catch (ArgumentNullException) { return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("Subject is not provided.")); }
        catch (InvalidOperationException) { return Task.FromResult<AuthenticateResult>(AuthenticateResult.Fail("Invalid subject provided.")); }

        ClaimsIdentity identity = new ClaimsIdentity(claims, Scheme.Name);

        ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(identity);

        AuthenticationTicket ticket = new AuthenticationTicket(claimPrincipal, authenticationScheme: "JWT");

        return Task.FromResult<AuthenticateResult>(AuthenticateResult.Success(ticket));
    }

    public string GenerateEmailVerificationJWT(string email) => GenerateJwt(new Claim[] { new Claim(ClaimTypes.Email, email) });

    public string GenerateAuthenticationJWT(string userId) => GenerateJwt(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public string GenerateJwt(Claim[] claims)
    {
        SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            IssuedAt = DateTime.UtcNow,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
            SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), _options.SecurityAlgorithm)
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = (new JwtSecurityTokenHandler());
        return jwtSecurityTokenHandler.WriteToken(jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor));
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidAudience = _options.Audience,
        ValidIssuer = _options.Issuer,
        IssuerSigningKey = GetSymmetricSecurityKey()
    };

    private SecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Convert.FromBase64String(_options.SecretKey));
}