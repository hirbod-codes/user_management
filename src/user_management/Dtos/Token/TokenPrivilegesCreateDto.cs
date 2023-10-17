namespace user_management.Dtos.Token;
using Bogus;
using Swashbuckle.AspNetCore.Filters;
using user_management.Data;
using user_management.Models;
using user_management.Validation.Attributes;

/// <summary>
/// This is where the third party client asks for its desired privileges, readFields, updateFields and whether it wants delete permission.
/// </summary>
public class TokenPrivilegesCreateDto : IExamplesProvider<TokenPrivilegesCreateDto>
{
    [PrivilegesValidation]
    public Privilege[] Privileges { get; set; } = Array.Empty<Privilege>();
    [ReaderFields]
    public Field[] ReadsFields { get; set; } = Array.Empty<Field>();
    [UpdaterFields]
    public Field[] UpdatesFields { get; set; } = Array.Empty<Field>();
    public bool DeletesUser { get; set; } = false;

    public TokenPrivilegesCreateDto GetExamples() => new()
    {
        Privileges = new Faker().PickRandom(StaticData.Privileges, 3).ToArray(),
        ReadsFields = new Faker().PickRandom(User.GetReadableFields(), 3).ToArray(),
        UpdatesFields = new Faker().PickRandom(User.GetUpdatableFields(), 3).ToArray(),
        DeletesUser = new Faker().Random.Bool()
    };
}
