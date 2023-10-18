using System.ComponentModel.DataAnnotations;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Validation.Attributes;

namespace user_management.Dtos.Client;

public class ClientDeleteDto : IExamplesProvider<ClientDeleteDto>
{
    [ObjectId]
    [MaxLength(25)]
    public string Id { get; set; } = null!;
    [MaxLength(1000)]
    public string Secret { get; set; } = null!;

    public ClientDeleteDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128)
    };
}
