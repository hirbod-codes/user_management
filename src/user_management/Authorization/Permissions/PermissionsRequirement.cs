namespace user_management.Authorization.Permissions;

using Microsoft.AspNetCore.Authorization;

public class PermissionsRequirement : IAuthorizationRequirement, IIdentifiable
{
    public Guid Identifier { get; set; }
    public string Permissions { get; }

    public PermissionsRequirement(string permissions, Guid identifier)
    {
        Permissions = permissions;
        Identifier = identifier;
    }

}