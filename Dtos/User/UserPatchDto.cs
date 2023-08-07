using user_management.Validation.Attributes;

namespace user_management.Dtos.User;

public class UserPatchDto
{
    [FiltersString]
    public string? FiltersString { get; set; }
    [UpdatesString]
    public string? UpdatesString { get; set; }
}