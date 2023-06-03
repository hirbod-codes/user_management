namespace user_management.Authorization.Scopes;

using Microsoft.AspNetCore.Authorization;

public class ScopesRequirement : IAuthorizationRequirement, IIdentifiable
{
    public Guid Identifier { get; set; }
    public string Scopes { get; }

    public ScopesRequirement(string permissions, Guid identifier)
    {
        Scopes = permissions;
        Identifier = identifier;
    }

}