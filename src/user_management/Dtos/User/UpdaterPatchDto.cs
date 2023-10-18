using user_management.Models;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class UpdaterPatchDto : IExamplesProvider<UpdaterPatchDto>
{
    [ObjectId]
    public string AuthorId { get; set; } = null!;
    [UpdaterAuthor]
    public string Author { get; set; } = null!;
    public bool IsPermitted { get; set; }
    [UpdaterFields]
    public Field[] Fields { get; set; } = Array.Empty<Field>();

    public UpdaterPatchDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Updater.USER, Updater.CLIENT }),
        IsPermitted = new Faker().Random.Bool(),
        Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 2).ToArray()
    };
}
