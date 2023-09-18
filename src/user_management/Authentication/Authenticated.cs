using System.Security.Authentication;

namespace user_management.Authentication;

public class Authenticated : IAuthenticated
{
    private string? _identifier = null;
    private string? _authenticationType = null;
    private string? _identifierToken = null;

    public string GetAuthenticatedIdentifier() => _identifier ?? throw new AuthenticationException();

    public string GetIdentifierToken() => _identifierToken ?? throw new AuthenticationException();

    public bool IsAuthenticated() => _identifier != null && _authenticationType != null && _identifierToken != null;

    public void SetAuthenticatedIdentifier(string identifier) => _identifier = identifier;

    public string GetAuthenticationType() => _authenticationType ?? throw new AuthenticationException();

    public void SetAuthenticationType(string authenticationType) => _authenticationType = authenticationType;

    public void SetIdentifierToken(string? token) => _identifierToken = token;
}
