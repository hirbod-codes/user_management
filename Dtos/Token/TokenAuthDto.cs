using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Token;

public class TokenAuthDto
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    [MinLength(40)]
    public string State { get; set; } = null!;
    public string CodeChallenge { get; set; } = null!;
    [StringIn(new string[] { "SHA256", "SHA512" })]
    public string CodeChallengeMethod { get; set; } = null!;
    public TokenPrivilegesCreateDto Scope { get; set; } = null!;
    public string ResponseType { get; set; } = null!;
}
