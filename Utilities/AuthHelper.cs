namespace user_management.Utilities;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using user_management.Authentication.JWT;

public class AuthHelper : IAuthHelper
{
    private readonly JWTAuthenticationOptions _options;

    public AuthHelper(IOptionsMonitor<JWTAuthenticationOptions> options)
    {
        _options = options.CurrentValue;
    }
    public string GenerateEmailVerificationJWT(string email) => GenerateJwt(new Claim[] { new Claim(ClaimTypes.Email, email) });

    public string GenerateAuthenticationJWT(string userId) => GenerateJwt(new Claim[] { new Claim(ClaimTypes.NameIdentifier, userId) });

    public string GenerateJwt(Claim[] claims)
    {
        SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor()
        {
            IssuedAt = DateTime.UtcNow,
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpireMinutes),
            SigningCredentials = new SigningCredentials(GetSymmetricSecurityKey(), _options.SecurityAlgorithm)
        };

        JwtSecurityTokenHandler jwtSecurityTokenHandler = (new JwtSecurityTokenHandler());
        return jwtSecurityTokenHandler.WriteToken(jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor));
    }

    private SecurityKey GetSymmetricSecurityKey() => new SymmetricSecurityKey(Convert.FromBase64String(_options.SecretKey));
}
