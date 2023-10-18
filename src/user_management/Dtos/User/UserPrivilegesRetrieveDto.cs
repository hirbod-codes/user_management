using user_management.Models;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class UserPrivilegesRetrieveDto : IExamplesProvider<UserPrivilegesRetrieveDto>
{
    public ReaderRetrieveDto[] Readers { get; set; } = Array.Empty<ReaderRetrieveDto>();
    public AllReaders? AllReaders { get; set; }
    public UpdaterRetrieveDto[] Updaters { get; set; } = Array.Empty<UpdaterRetrieveDto>();
    public AllUpdaters? AllUpdaters { get; set; }
    public DeleterRetrieveDto[] Deleters { get; set; } = Array.Empty<DeleterRetrieveDto>();

    public UserPrivilegesRetrieveDto GetExamples() => new()
    {
        Readers = new ReaderRetrieveDto[] { new ReaderRetrieveDto().GetExamples(), new ReaderRetrieveDto().GetExamples() },
        AllReaders = new()
        {
            ArePermitted = new Faker().Random.Bool(),
            Fields = new Faker().PickRandom(Models.User.GetReadableFields(), 3).ToArray()
        },
        Updaters = new UpdaterRetrieveDto[] { new UpdaterRetrieveDto().GetExamples(), new UpdaterRetrieveDto().GetExamples() },
        AllUpdaters = new()
        {
            ArePermitted = new Faker().Random.Bool(),
            Fields = new Faker().PickRandom(Models.User.GetUpdatableFields(), 3).ToArray()
        },
        Deleters = new DeleterRetrieveDto[] { new DeleterRetrieveDto().GetExamples(), new DeleterRetrieveDto().GetExamples() },
    };
}
