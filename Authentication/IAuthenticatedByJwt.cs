using user_management.Models;

namespace user_management.Authentication;

public interface IAuthenticatedByJwt
{
    /// <returns>True id there is an authenticated entity, false otherwise.</returns>
    public bool IsAuthenticated();

    /// <returns>The Authenticated User</returns>
    /// <exception cref="System.Security.Authentication.AuthenticationException"></exception>
    public Task<User> GetAuthenticated();

    public void SetAuthenticated(User authenticatedUser);
}
