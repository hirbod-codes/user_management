using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Client;

public class ClientPatchDto : IExamplesProvider<ClientPatchDto>
{
    [ObjectId]
    [Required]
    [MaxLength(25)]
    public string Id { get; set; } = null!;

    [Required]
    [MaxLength(1000)]
    public string Secret { get; set; } = null!;

    [Required]
    [MaxLength(250)]
    public string RedirectUrl { get; set; } = null!;

    public ClientPatchDto GetExamples() => new()
    {
        Id = new Faker().Random.String2(24, "0123456789"),
        Secret = new Faker().Random.String2(128),
        RedirectUrl = new Faker().Internet.Url()
    };
}
