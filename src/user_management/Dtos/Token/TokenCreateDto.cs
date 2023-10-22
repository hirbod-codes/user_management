using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

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
    [MaxLength(1000)]
    public string Code { get; set; } = null!;
    [ObjectId]
    [MaxLength(25)]
    public string ClientId { get; set; } = null!;
    [MaxLength(250)]
    public string RedirectUrl { get; set; } = null!;
    /// <summary>
    /// The string client hashed and base64 encoded with and provided as CodeChallenge.
    /// </summary>
    [MaxLength(1000)]
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
