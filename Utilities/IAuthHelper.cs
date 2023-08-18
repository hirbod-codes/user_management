using System.Security.Claims;

namespace user_management.Utilities;

public interface IAuthHelper
{
    public string GetAuthenticationType(ClaimsPrincipal user);

    /// <summary>
    /// Get userId if authenticated with jwt token, otherwise gets clientId.
    /// </summary>
    public Task<string?> GetIdentifier(ClaimsPrincipal user);
}