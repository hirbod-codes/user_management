namespace user_management.Dtos.Token;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

public class TokenRetrieveDto : IExamplesProvider<TokenRetrieveDto>
{
    public string Token { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

    public TokenRetrieveDto GetExamples() => new()
    {
        Token = new Faker().Random.String2(128),
        RefreshToken = new Faker().Random.String2(128)
    };
}
