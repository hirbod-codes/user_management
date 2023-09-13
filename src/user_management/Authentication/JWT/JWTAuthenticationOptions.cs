namespace user_management.Authentication.JWT;

using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

public class JWTAuthenticationOptions : AuthenticationSchemeOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public int ExpireMinutes { get; set; } = 10080;
    public string SecurityAlgorithm { get; set; } = SecurityAlgorithms.HmacSha256Signature;
}