using user_management.Models;

namespace user_management.Authentication;

public interface IAuthenticatedByBearer
{
    /// <returns>True id there is an authenticated entity, false otherwise.</returns>
    public bool IsAuthenticated();

    /// <returns>The Authenticated Client</returns>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public Task<AuthorizedClient> GetAuthenticated();

    public void SetAuthenticated(AuthorizedClient authenticatedAuthorizedClient);
}
