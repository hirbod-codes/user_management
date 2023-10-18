using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class Activation : IExamplesProvider<Activation>
{
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;

    public Activation GetExamples() => new()
    {
        Email = new Faker().Internet.ExampleEmail(),
        Password = new Faker().Internet.Password(),
        VerificationSecret = new Faker().Random.String2(6, "0123456789")
    };
}
