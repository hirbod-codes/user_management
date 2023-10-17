using MongoDB.Bson;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientExposedDto : IExamplesProvider<ClientExposedDto>
{
    [ObjectId]
    public string ClientId { get; set; } = null!;
    public string Secret { get; set; } = null!;

    public ClientExposedDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128)
    };
}
