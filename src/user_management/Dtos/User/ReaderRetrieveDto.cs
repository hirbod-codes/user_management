using user_management.Models;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class ReaderRetrieveDto : IExamplesProvider<ReaderRetrieveDto>
{
    [ObjectId]
    public string? AuthorId { get; set; }
    [ReaderAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
    [ReaderFields]
    public Field[]? Fields { get; set; } = null!;

    public ReaderRetrieveDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Reader.USER, Reader.CLIENT }),
        IsPermitted = new Faker().Random.Bool(),
        Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 2).ToArray()
    };
}
