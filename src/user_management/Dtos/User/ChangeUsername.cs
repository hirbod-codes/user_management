using System.ComponentModel.DataAnnotations;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ChangeUsername : IExamplesProvider<ChangeUsername>
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string Username { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;

    public ChangeUsername GetExamples() => new()
    {
        Email = new Faker().Internet.ExampleEmail(),
        Username = new Faker().Internet.UserName(),
        VerificationSecret = new Faker().Random.String2(6, "0123456789")
    };
}
