using System.Security.Authentication;
using user_management.Models;

namespace user_management.Authentication.Bearer;

public class AuthenticatedByBearer : IAuthenticatedByBearer
{
    private AuthorizedClient? _userClient = null;

    public Task<AuthorizedClient> GetAuthenticated() => _userClient != null ? Task.FromResult<AuthorizedClient>(_userClient) : throw new AuthenticationException();

    public bool IsAuthenticated() => _userClient != null;

    public void SetAuthenticated(AuthorizedClient authenticatedClient) => _userClient = authenticatedClient;
}
