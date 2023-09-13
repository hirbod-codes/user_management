using System.Security.Authentication;
using user_management.Models;

namespace user_management.Authentication.Bearer;

public class AuthenticatedByBearer : IAuthenticatedByBearer
{
    private UserClient? _userClient = null;

    public Task<UserClient> GetAuthenticated() => _userClient != null ? Task.FromResult<UserClient>(_userClient) : throw new AuthenticationException();

    public bool IsAuthenticated() => _userClient != null;

    public void SetAuthenticated(UserClient authenticatedClient) => _userClient = authenticatedClient;
}