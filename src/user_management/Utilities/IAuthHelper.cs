namespace user_management.Utilities;

public interface IAuthHelper
{
    public string GenerateEmailVerificationJWT(string email);
    public string GenerateAuthenticationJWT(string userId);
}
