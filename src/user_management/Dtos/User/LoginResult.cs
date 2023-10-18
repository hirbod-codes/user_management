namespace user_management.Dtos.User;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

public class LoginResult : IExamplesProvider<LoginResult>
{
    public string UserId { get; set; } = null!;
    public string Jwt { get; set; } = null!;

    public LoginResult GetExamples() => new()
    {
        UserId = new Faker().Random.String2(24, "0123456789"),
        Jwt = new Faker().Random.String2(128)
    };
}
