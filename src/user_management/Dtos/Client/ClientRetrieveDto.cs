using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientRetrieveDto : IExamplesProvider<ClientRetrieveDto>
{
    [ObjectId]
    public string? Id { get; set; }
    public string? Secret { get; set; }
    public string? RedirectUrl { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ClientRetrieveDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128),
        RedirectUrl = new Faker().Internet.Url(),
        UpdatedAt = new Faker().Date.Between(DateTime.UtcNow, DateTime.UtcNow.AddDays(-2)),
        CreatedAt = new Faker().Date.Between(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-5))
    };
}
