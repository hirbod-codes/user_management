using System.Security.Authentication;

namespace user_management.Authentication;

public class Authenticated : IAuthenticated
{
    private string? _identifier = null;
    private string? _authenticationType = null;

    public string GetAuthenticatedIdentifier() => _identifier == null ? throw new AuthenticationException() : _identifier;

    public bool IsAuthenticated() => _identifier != null;

    public void SetAuthenticatedIdentifier(string identifier) => _identifier = identifier;

    public string GetAuthenticationType() => _authenticationType == null ? throw new AuthenticationException() : _authenticationType;

    public void SetAuthenticationType(string authenticationType) => _authenticationType = authenticationType;
}