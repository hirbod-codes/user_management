using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ChangePhoneNumber : IExamplesProvider<ChangePhoneNumber>
{
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [RegEx(Models.User.PHONE_NUMBER_REGEX)]
    [MaxLength(100)]
    public string PhoneNumber { get; set; } = null!;
    [MinLength(6)]
    public string VerificationSecret { get; set; } = null!;

    public ChangePhoneNumber GetExamples() => new()
    {
        Email = new Faker().Internet.ExampleEmail(),
        PhoneNumber = new Faker().Phone.PhoneNumber(),
        VerificationSecret = new Faker().Random.String2(6, "0123456789")
    };
}
