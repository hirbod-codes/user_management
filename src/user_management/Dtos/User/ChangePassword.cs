using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

[PasswordConfirmationAttribute<ChangePassword>]
public class ChangePassword : IPasswordConfirmable, IExamplesProvider<ChangePassword>
{
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;
    [Password]
    public string PasswordConfirmation { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;

    public ChangePassword GetExamples()
    {
        string password = new Faker().Internet.ExampleEmail();
        return new()
        {
            Email = new Faker().Internet.ExampleEmail(),
            Password = password,
            PasswordConfirmation = password,
            VerificationSecret = new Faker().Random.String2(6, "0123456789")
        };
    }
}
