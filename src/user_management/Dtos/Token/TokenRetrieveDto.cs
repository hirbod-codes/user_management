namespace user_management.Dtos.Token;

public class TokenRetrieveDto
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}
