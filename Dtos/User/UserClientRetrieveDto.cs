namespace user_management.Models;

public class UserClientRetrieveDto
{
    public string? ClientId { get; set; }
    public RefreshToken? RefreshToken { get; set; }
    public Token? Token { get; set; }
}