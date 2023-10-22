using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class UserCreateDto : IExamplesProvider<UserCreateDto>
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    [EmailAddress]
    [MaxLength(300)]
    public string Email { get; set; } = null!;
    [RegEx(Models.User.PHONE_NUMBER_REGEX)]
    [MaxLength(100)]
    public string? PhoneNumber { get; set; }
    [MinLength(3)]
    public string Username { get; set; } = null!;
    [Password]
    public string Password { get; set; } = null!;

    public UserCreateDto GetExamples() => new()
    {
        FirstName = new Faker().Person.FirstName,
        MiddleName = new Faker().Person.FirstName,
        LastName = new Faker().Person.LastName,
        Email = new Faker().Internet.ExampleEmail(),
        PhoneNumber = new Faker().Phone.PhoneNumber(),
        Username = new Faker().Internet.UserName(),
        Password = new Faker().Internet.Password()
    };
}
