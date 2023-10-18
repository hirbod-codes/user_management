using user_management.Models;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class UserPrivilegesPatchDto : IExamplesProvider<UserPrivilegesPatchDto>
{
    public ReaderPatchDto[]? Readers { get; set; } = null;
    public AllReaders? AllReaders { get; set; } = null;
    public UpdaterPatchDto[]? Updaters { get; set; } = null;
    public AllUpdaters? AllUpdaters { get; set; } = null;
    public DeleterPatchDto[]? Deleters { get; set; } = null;

    public UserPrivilegesPatchDto GetExamples() => new()
    {
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
