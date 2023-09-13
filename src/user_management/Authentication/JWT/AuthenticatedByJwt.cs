using System.Security.Authentication;
using user_management.Models;

namespace user_management.Authentication.JWT;

public class AuthenticatedByJwt : IAuthenticatedByJwt
{
    private User? _user = null;

    public Task<User> GetAuthenticated() => _user != null ? Task.FromResult<User>(_user) : throw new AuthenticationException();

    public bool IsAuthenticated() => _user != null;

    public void SetAuthenticated(User authenticatedUser) => _user = authenticatedUser;
}
