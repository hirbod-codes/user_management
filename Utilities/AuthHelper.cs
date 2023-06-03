namespace user_management.Utilities;

using System.Security.Claims;
using user_management.Data.User;
using user_management.Models;

public class AuthHelper : IAuthHelper
{
    public string GetAuthenticationType(ClaimsPrincipal user)
    {
        if (user.Identity!.AuthenticationType == "JWT") return "JWT";
        if (user.Identity!.AuthenticationType == "Bearer") return "Bearer";
        throw new UnauthorizedAccessException();
    }

    public async Task<string?> GetIdentifier(ClaimsPrincipal user, IUserRepository userRepository)
    {
        Claim? claim = user.Claims.ToList().FirstOrDefault<Claim?>(c => c != null && c.Type == ClaimTypes.NameIdentifier, null);
        if (claim == null) return null;

        if (user.Identity!.AuthenticationType == "JWT")
            return claim.Value;
        else
            try
            {
                return (await userRepository.RetrieveByTokenValue(claim.Value))!.Clients.ToList().FirstOrDefault<UserClient?>(c => c != null && c.Token != null && c.Token.Value == claim.Value, null)!.ClientId.ToString();
            }
            catch (NullReferenceException) { return null; }
    }
}