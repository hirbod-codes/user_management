using user_management.Models;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ReaderPatchDto : IExamplesProvider<ReaderPatchDto>
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [ReaderAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
    [ReaderFields]
    public Field[] Fields { get; set; } = Array.Empty<Field>();

    public ReaderPatchDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Reader.USER, Reader.CLIENT }),
        IsPermitted = new Faker().Random.Bool(),
        Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 2).ToArray()
    };
}
