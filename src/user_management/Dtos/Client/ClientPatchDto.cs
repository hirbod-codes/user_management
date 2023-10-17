using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Client;

public class ClientPatchDto : IExamplesProvider<ClientPatchDto>
{
    [ObjectId]
    [Required]
    public string Id { get; set; } = null!;

    [Required]
    public string Secret { get; set; } = null!;

    [Required]
    public string RedirectUrl { get; set; } = null!;

    public ClientPatchDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128),
        RedirectUrl = new Faker().Internet.Url()
    };
}
