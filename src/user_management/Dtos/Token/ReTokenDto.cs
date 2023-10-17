using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Token;

public class ReTokenDto : IExamplesProvider<ReTokenDto>
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;

    public ReTokenDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        ClientSecret = new Faker().Random.String2(128),
        RefreshToken = new Faker().Random.String2(128)
    };
}
