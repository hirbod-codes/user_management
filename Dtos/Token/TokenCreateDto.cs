using user_management.Validation.Attributes;

namespace user_management.Dtos.Token;

public class TokenCreateDto
{
    public string GrantType { get; set; } = null!;
    public string Code { get; set; } = null!;
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public string CodeVerifier { get; set; } = null!;
}