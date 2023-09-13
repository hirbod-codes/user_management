namespace user_management.Authorization;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using user_management.Authorization.Attributes;
using user_management.Authorization.Permissions;
using user_management.Authorization.Roles;
using user_management.Authorization.Scopes;

public class PermissionsPolicyProvider : IAuthorizationPolicyProvider
{
    public DefaultAuthorizationPolicyProvider FallbackPolicyProvider { get; }

    public PermissionsPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        FallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return Task.FromResult(new AuthorizationPolicyBuilder(new string[] { "Bearer", "JWT" }).RequireAuthenticatedUser().Build());
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return FallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            return FallbackPolicyProvider.GetPolicyAsync(policyName);

        string[] policyTokens = policyName.Split(';', StringSplitOptions.RemoveEmptyEntries);

        if (policyTokens?.Any() != true)
            return FallbackPolicyProvider.GetPolicyAsync(policyName);

        AuthorizationPolicyBuilder policy = new AuthorizationPolicyBuilder(new string[] { "Bearer", "JWT" });
        Guid identifier = Guid.NewGuid();

        foreach (string token in policyTokens)
        {
            string[] pair = token.Split('$', StringSplitOptions.RemoveEmptyEntries);

            if (pair?.Any() != true || pair.Length != 2)
            {
                return FallbackPolicyProvider.GetPolicyAsync(policyName);
            }

            IAuthorizationRequirement? requirement = (pair[0]) switch
            {
                PermissionsAttribute.PermissionsGroup => new PermissionsRequirement(pair[1], identifier),
                PermissionsAttribute.RolesGroup => new RolesRequirement(pair[1], identifier),
                PermissionsAttribute.ScopesGroup => new ScopesRequirement(pair[1], identifier),
                _ => null,
            };

            if (requirement == null)
                return FallbackPolicyProvider.GetPolicyAsync(policyName);

            policy.AddRequirements(requirement);
        }

        return Task.FromResult<AuthorizationPolicy?>(policy.Build());
    }
}