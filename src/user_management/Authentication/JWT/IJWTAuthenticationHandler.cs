namespace user_management.Authentication.JWT;

using System.Security.Claims;

public interface IJWTAuthenticationHandler
{
    public string GenerateEmailVerificationJWT(string email);
    public string GenerateAuthenticationJWT(string userId);
    public string GenerateJwt(Claim[] claims);
}