using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.Token;

public class ReTokenDto : IExamplesProvider<ReTokenDto>
{
    [ObjectId]
    [MaxLength(1000)]
    public string ClientId { get; set; } = null!;
    [MaxLength(1000)]
    public string ClientSecret { get; set; } = null!;
    [MaxLength(1000)]
    public string RefreshToken { get; set; } = null!;

    public ReTokenDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        ClientSecret = new Faker().Random.String2(128),
        RefreshToken = new Faker().Random.String2(128)
    };
}
