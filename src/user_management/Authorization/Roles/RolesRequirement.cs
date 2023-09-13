namespace user_management.Authorization.Roles;

using Microsoft.AspNetCore.Authorization;

public class RolesRequirement : IAuthorizationRequirement, IIdentifiable
{
    public Guid Identifier { get; set; }
    public string Roles { get; }

    public RolesRequirement(string permissions, Guid identifier)
    {
        Roles = permissions;
        Identifier = identifier;
    }

}