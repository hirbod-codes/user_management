using user_management.Models;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace user_management.Dtos.User;

public class UserPrivilegesPatchDto : IExamplesProvider<UserPrivilegesPatchDto>
{
    /// <summary>
    /// ID of the user that is going to be updated.
    /// </summary>
    [MaxLength(25)]
    public string UserId { get; set; } = null!;
    [MaxLength(100)]
    public ReaderPatchDto[]? Readers { get; set; } = null;
    public AllReaders? AllReaders { get; set; } = null;
    [MaxLength(100)]
    public UpdaterPatchDto[]? Updaters { get; set; } = null;
    public AllUpdaters? AllUpdaters { get; set; } = null;
    [MaxLength(100)]
    public DeleterPatchDto[]? Deleters { get; set; } = null;

    public UserPrivilegesPatchDto GetExamples() => new()
    {
        UserId = new Faker().Random.String2(24, "0123456789"),
        Readers = new ReaderPatchDto[] { new ReaderPatchDto().GetExamples(), new ReaderPatchDto().GetExamples() },
        AllReaders = new()
        {
            ArePermitted = new Faker().Random.Bool(),
            Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 3).ToArray()
        },
        Updaters = new UpdaterPatchDto[] { new UpdaterPatchDto().GetExamples(), new UpdaterPatchDto().GetExamples() },
        AllUpdaters = new()
        {
            ArePermitted = new Faker().Random.Bool(),
            Fields = new Faker().PickRandom(Models.User.GetUpdatableFields(), 3).ToArray()
        },
        Deleters = new DeleterPatchDto[] { new DeleterPatchDto().GetExamples(), new DeleterPatchDto().GetExamples() },
    };
}
