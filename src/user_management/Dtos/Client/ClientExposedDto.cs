using MongoDB.Bson;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Validation.Attributes;
using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.Client;

public class ClientExposedDto : IExamplesProvider<ClientExposedDto>
{
    [ObjectId]
    [MaxLength(25)]
    public string ClientId { get; set; } = null!;
    [MaxLength(1000)]
    public string Secret { get; set; } = null!;

    public ClientExposedDto GetExamples() => new()
    {
        ClientId = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128)
    };
}
