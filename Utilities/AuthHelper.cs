namespace user_management.Utilities;

using System.Security.Claims;
using user_management.Models;
using user_management.Services.Data.User;

public class AuthHelper : IAuthHelper
{
    private readonly IUserRepository _userRepository;

    public AuthHelper(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    public string GetAuthenticationType(ClaimsPrincipal user)
    {
        if (user.Identity!.AuthenticationType == "JWT") return "JWT";
        if (user.Identity!.AuthenticationType == "Bearer") return "Bearer";
        throw new UnauthorizedAccessException();
    }

    public async Task<string?> GetIdentifier(ClaimsPrincipal user)
    {
        Claim? claim = user.Claims.ToList().FirstOrDefault<Claim?>(c => c != null && c.Type == ClaimTypes.NameIdentifier, null);
        if (claim == null) return null;

        if (user.Identity!.AuthenticationType == "JWT")
            return claim.Value;
        else
            try
            {
                return (await _userRepository.RetrieveByTokenValue(claim.Value))!.Clients.ToList().FirstOrDefault<UserClient?>(c => c != null && c.Token != null && c.Token.Value == claim.Value, null)!.ClientId.ToString();
            }
            catch (NullReferenceException) { return null; }
    }
}