using user_management.Validation.Attributes;

namespace user_management.Dtos.Token;

public class TokenAuthDto
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    public string State { get; set; } = null!;
    public string CodeChallenge { get; set; } = null!;
    public string CodeChallengeMethod { get; set; } = null!;
    public TokenPrivilegesCreateDto Scope { get; set; } = null!;
    public string ResponseType { get; set; } = null!;
}
