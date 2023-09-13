namespace user_management.Authentication;

public interface IAuthenticated
{
    public bool IsAuthenticated();

    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public string GetAuthenticatedIdentifier();

    public void SetAuthenticatedIdentifier(string identifier);

    public void SetAuthenticationType(string authenticationType);
    
    public string GetAuthenticationType();
}
