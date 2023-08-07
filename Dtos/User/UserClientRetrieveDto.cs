using user_management.Validation.Attributes;

namespace user_management.Models;

public class UserClientRetrieveDto
{
    [ObjectId]
    public string? ClientId { get; set; }
    public RefreshToken? RefreshToken { get; set; }
    public Token? Token { get; set; }
}