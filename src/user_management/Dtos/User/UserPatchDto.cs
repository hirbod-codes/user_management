using user_management.Validation.Attributes;
using Bogus;
using Swashbuckle.AspNetCore.Filters;

namespace user_management.Dtos.User;

public class UserPatchDto : IExamplesProvider<UserPatchDto>
{
    /// <summary>
    /// <include file='./docs/xml.xml' path='user_management/Data/Logics/Filter/FilterLogicsGeneric/BuildILogic/logicsString' />
    /// </summary>
    [FiltersString]
    public string? FiltersString { get; set; }

    /// <summary>
    /// <include file='./docs/xml.xml' path='user_management/Data/Logics/Update/UpdateLogicsGeneric/BuildILogic/updatesString' />
    /// </summary>
    [UpdatesString]
    public string? UpdatesString { get; set; }

    public UserPatchDto GetExamples() => new()
    {
        FiltersString = "Username::Ne::mike::string",
        UpdatesString = "Username::Set::John::string"
    };
}
