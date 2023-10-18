using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class Login : IExamplesProvider<Login>
{
    public string? Username { get; set; } = null;
    [EmailAddress]
    [MaxLength(300)]
    public string? Email { get; set; } = null;
    [Password]
    public string Password { get; set; } = null!;

    public Login GetExamples() => new()
    {
        Username = new Faker().Internet.UserName(),
        Email = new Faker().Internet.ExampleEmail(),
        Password = new Faker().Internet.Password()
    };
}
