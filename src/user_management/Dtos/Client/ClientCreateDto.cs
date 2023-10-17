using System.ComponentModel.DataAnnotations;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.Client;

public class ClientCreateDto: IExamplesProvider<ClientCreateDto>
{
    [Required]
    public string RedirectUrl { get; set; } = null!;

    public ClientCreateDto GetExamples() => new() { RedirectUrl = new Faker().Internet.Url() };
}
