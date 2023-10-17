using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientDeleteDto : IExamplesProvider<ClientDeleteDto>
{
    [ObjectId]
    public string Id { get; set; } = null!;
    public string Secret { get; set; } = null!;

    public ClientDeleteDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128)
    };
}
