using user_management.Validation.Attributes;

namespace user_management.Dtos.Token;

public class ReTokenDto
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
}