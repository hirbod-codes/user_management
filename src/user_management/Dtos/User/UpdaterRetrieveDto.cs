using user_management.Models;
using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.User;

public class UpdaterRetrieveDto : IExamplesProvider<UpdaterRetrieveDto>
{
    [ObjectId]
    [MaxLength(25)]
    public string? AuthorId { get; set; }
    [UpdaterAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }
    [UpdaterFields]
    public Field[]? Fields { get; set; } = null!;

    public UpdaterRetrieveDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Updater.USER, Updater.CLIENT }),
        IsPermitted = new Faker().Random.Bool(),
        Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 2).ToArray()
    };
}
