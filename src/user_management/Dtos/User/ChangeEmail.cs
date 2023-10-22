using System.ComponentModel.DataAnnotations;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ChangeEmail : IExamplesProvider<ChangeEmail>
{
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [EmailAddress]
    [MaxLength(300)]
    public string NewEmail { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;

    public ChangeEmail GetExamples() => new()
    {
        Email = new Faker().Internet.ExampleEmail(),
        NewEmail = new Faker().Internet.ExampleEmail(),
        VerificationSecret = new Faker().Random.String2(6, "0123456789")
    };
}
