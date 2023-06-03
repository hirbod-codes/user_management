namespace user_management.Dtos.Token;

using user_management.Models;

public class ReTokenDto
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}