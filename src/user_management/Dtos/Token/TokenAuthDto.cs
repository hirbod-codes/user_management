using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Token;

public class TokenAuthDto : IExamplesProvider<TokenAuthDto>
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string RedirectUrl { get; set; } = null!;
    /// <summary>
    /// A string provided by third party client.
    /// </summary>
    /// <value></value>
    [MinLength(40)]
    public string State { get; set; } = null!;
    public string CodeChallenge { get; set; } = null!;
    /// <summary>
    /// The hash method that has been used to hash the code verifier.
    /// </summary>
    [StringIn(new string[] { "SHA256", "SHA512" })]
    public string CodeChallengeMethod { get; set; } = null!;
    public TokenPrivilegesCreateDto Scope { get; set; } = null!;
    /// <summary>
    /// The only supported value currently is 'code'
    /// </summary>
    public string ResponseType { get; set; } = null!;

    public TokenAuthDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        RedirectUrl = new Faker().Internet.Url(),
        State = new Faker().Random.String2(40),
        CodeChallenge = new Faker().Random.String2(60),
        CodeChallengeMethod = new Faker().PickRandom(new string[] { "SHA256", "SHA512" }),
        Scope = new TokenPrivilegesCreateDto().GetExamples(),
        ResponseType = "code",
    };
}
