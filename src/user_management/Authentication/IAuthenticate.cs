namespace user_management.Authentication;

public interface IAuthenticated
{
    public bool IsAuthenticated();

    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public string GetAuthenticatedIdentifier();

    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public string GetIdentifierToken();

    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public string GetAuthenticationType();

    public void SetAuthenticatedIdentifier(string identifier);

    public void SetAuthenticationType(string authenticationType);

    public void SetIdentifierToken(string token);
}
