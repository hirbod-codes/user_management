using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class UserPatchDto
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
}