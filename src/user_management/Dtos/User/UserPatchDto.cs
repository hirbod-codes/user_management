using Swashbuckle.AspNetCore.Filters;
using user_management.Data.Logics;
using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class UserPatchDto : IExamplesProvider<UserPatchDto>
{
    [MassReadable]
    public Filter? Filters { get; set; }

    [MassUpdatable]
    public IEnumerable<Update> Updates { get; set; } = null!;

    public UserPatchDto GetExamples() => new()
    {
        Filters = new() { Field = "Username", Operation = Filter.NE, Value = "mike", Type = Types.STRING },
        Updates = new Update[] { new() { Field = "Username", Operation = Update.SET, Value = "John", Type = Types.STRING } }
    };
}
