using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Models;

namespace user_management.Dtos.User;

public class DeleterPatchDto : IExamplesProvider<DeleterPatchDto>
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [DeleterAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }

    public DeleterPatchDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Deleter.AUTHOR, Deleter.USER }),
        IsPermitted = new Faker().Random.Bool()
    };
}
