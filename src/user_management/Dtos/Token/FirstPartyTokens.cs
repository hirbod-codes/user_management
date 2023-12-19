namespace user_management.Dtos.Token;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

public class FirstPartyTokens : IExamplesProvider<FirstPartyTokens>
{
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string TargetUserId { get; set; } = null!;

    public FirstPartyTokens GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24),
        ClientSecret = new Faker().Random.String2(24),
        TargetUserId = new Faker().Random.String2(24)
    };
}
