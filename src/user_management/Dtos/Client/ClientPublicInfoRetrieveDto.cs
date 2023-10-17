using Bogus;
using user_management.Validation.Attributes;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Client;

public class ClientPublicInfoRetrieveDto : IExamplesProvider<ClientPublicInfoRetrieveDto>
{
    [ObjectId]
    public string? Id { get; set; }
    public string? RedirectUrl { get; set; } = null!;
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ClientPublicInfoRetrieveDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        RedirectUrl = new Faker().Internet.Url(),
        UpdatedAt = new Faker().Date.Between(DateTime.UtcNow, DateTime.UtcNow.AddDays(-2)),
        CreatedAt = new Faker().Date.Between(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-5))
    };
}
