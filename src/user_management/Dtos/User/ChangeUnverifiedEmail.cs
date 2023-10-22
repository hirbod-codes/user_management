using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ChangeUnverifiedEmail : IExamplesProvider<ChangeUnverifiedEmail>
{
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [EmailAddress]
    [MaxLength(300)]
    public string NewEmail { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;

    public ChangeUnverifiedEmail GetExamples() => new()
    {
        Email = new Faker().Internet.ExampleEmail(),
        NewEmail = new Faker().Internet.ExampleEmail(),
        Password = new Faker().Internet.Password()
    };
}
