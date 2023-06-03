namespace user_management.Dtos.Token;

public class TokenCreateDto
{
    public string GrantType { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public string CodeVerifier { get; set; } = null!;
}