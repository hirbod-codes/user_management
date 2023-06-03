namespace user_management.Authentication.Bearer;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using user_management.Data.User;
using user_management.Models;
using user_management.Utilities;

public class BearerAuthenticationHandler : AuthenticationHandler<BearerAuthenticationOptions>
{
    private readonly IUserRepository _userRepository;
    private BearerAuthenticationOptions _options;

    public BearerAuthenticationHandler(IUserRepository repo, IOptionsMonitor<BearerAuthenticationOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
    {
        _options = options.CurrentValue;
        _userRepository = repo;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers["Authorization"], out var authenticationHeaderValue))
            return AuthenticateResult.NoResult();

        string[] authenticationHeaderParts = authenticationHeaderValue.ToString().Split(" ");
        string tokenValue = authenticationHeaderParts[1];

        if (authenticationHeaderParts[0] != "Bearer")
            return AuthenticateResult.Fail("No Bearer token provided.");

        string? hashedToken = (new StringHelper()).HashWithoutSalt(tokenValue);
        if (hashedToken == null)
            return AuthenticateResult.Fail("The authorization token not found.");

        User? user = await _userRepository.RetrieveByTokenValue(hashedToken);
        if (user == null)
            return AuthenticateResult.Fail("The authorization token not found.");

        List<UserClient> userClients = user.Clients.ToList();
        UserClient? userClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null && uc.Token != null && uc.Token.Value == hashedToken, null);
        if (userClient == null)
            return AuthenticateResult.Fail("The authorization token not found.");

        if (userClient.Token!.ExpirationDate <= DateTime.UtcNow)
            return AuthenticateResult.Fail("The authorization token has been expired.");

        if ((bool)userClient.Token.IsRevoked! || !userClient.RefreshToken!.IsVerified)
            return AuthenticateResult.Fail("The authorization token has been revoked.");

        return Success(userClient.Token.Value!.ToString()!);
    }

    private AuthenticateResult Success(string tokenValue)
    {
        List<Claim> claims = new List<Claim>() { new Claim(ClaimTypes.NameIdentifier, tokenValue) };

        ClaimsIdentity identity = new ClaimsIdentity(claims, "Bearer");

        ClaimsPrincipal claimPrincipal = new ClaimsPrincipal(identity);

        AuthenticationTicket ticket = new AuthenticationTicket(claimPrincipal, authenticationScheme: "Bearer");

        return AuthenticateResult.Success(ticket);
    }
}