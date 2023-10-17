using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Token;

public class TokenCreateDto : IExamplesProvider<TokenCreateDto>
{
    /// <summary>
    /// The only supported value currently is 'authorization_code'
    /// </summary>
    public string GrantType { get; set; } = null!;
    /// <summary>
    /// The string that client received from this server as code.
    /// </summary>
    public string Code { get; set; } = null!;
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    /// <summary>
    /// What client hashed, base64 encoded and provided as CodeChallenge.
    /// </summary>
    public string CodeVerifier { get; set; } = null!;

    public TokenCreateDto GetExamples() => new()
    {
        GrantType = "authorization_code",
        Code = new Faker().Random.String2(40),
        ClientId = new Faker().Random.String2(24, "0123456789"),
        RedirectUrl = new Faker().Internet.Url(),
        CodeVerifier = new Faker().Random.String2(50)
    };
}
