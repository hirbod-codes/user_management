using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Models;
using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.User;

public class DeleterRetrieveDto : IExamplesProvider<DeleterRetrieveDto>
{
    [ObjectId]
    [MaxLength(25)]
    public string? AuthorId { get; set; }
    [DeleterAuthor]
    public string? Author { get; set; }
    public bool? IsPermitted { get; set; }

    public DeleterRetrieveDto GetExamples() => new()
    {
        AuthorId = new Faker().Random.String2(24, "0123456789"),
        Author = new Faker().PickRandom(new string[] { Deleter.AUTHOR, Deleter.USER }),
        IsPermitted = new Faker().Random.Bool()
    };
}
