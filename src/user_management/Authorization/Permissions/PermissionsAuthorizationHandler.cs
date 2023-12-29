namespace user_management.Authorization.Permissions;

using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using user_management.Models;
using user_management.Services.Data.User;
using user_management.Authentication;

public class PermissionsAuthorizationHandler : AuthorizationHandler<PermissionsRequirement>
{
    private readonly ILogger<PermissionsAuthorizationHandler> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticatedByJwt _authenticatedByJwt;
    private readonly IAuthenticatedByBearer _authenticatedByBearer;

    public PermissionsAuthorizationHandler(ILogger<PermissionsAuthorizationHandler> logger, IUserRepository userRepository, IAuthenticatedByJwt authenticatedByJwt, IAuthenticatedByBearer authenticatedByBearer)
    {
        _logger = logger;
        _userRepository = userRepository;
        _authenticatedByJwt = authenticatedByJwt;
        _authenticatedByBearer = authenticatedByBearer;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionsRequirement requirement)
    {
        if (context.User == null || context.User.Identity == null || !context.User.Identity.IsAuthenticated)
            return Task.CompletedTask;

        if (context.HasSucceeded)
            return Task.CompletedTask;

        if (context.User == null || requirement == null || string.IsNullOrWhiteSpace(requirement.Permissions))
            return Task.CompletedTask;

        string? id = context.User.Claims?.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
        if (id == null)
            return Task.CompletedTask;

        if (context.User.Identity.AuthenticationType == "JWT")
            return AuthorizeJWT(context, requirement);
        else
            return AuthorizeBearer(id, context, requirement);
    }

    private async Task AuthorizeJWT(AuthorizationHandlerContext context, PermissionsRequirement requirement)
    {
        string[] requirementTokens = requirement.Permissions.Split("|", StringSplitOptions.RemoveEmptyEntries);
        if (requirementTokens?.Any() != true) return;

        if (!_authenticatedByJwt.IsAuthenticated()) return;
        List<Privilege> privileges = (await _authenticatedByJwt.GetAuthenticated()).Privileges.ToList();
        if (privileges.Count == 0) return;

        foreach (string requirementToken in requirementTokens)
            if (privileges.FirstOrDefault<Privilege?>(p => p != null && p.Name == requirementToken && p.Value != null && (bool)p.Value == true, null) != null)
            {
                Utility.Succeed(context, requirement.Identifier);
                break;
            }

        return;
    }

    private async Task AuthorizeBearer(string tokenValue, AuthorizationHandlerContext context, PermissionsRequirement requirement)
    {
        string[] requirementTokens = requirement.Permissions.Split("|", StringSplitOptions.RemoveEmptyEntries);
        if (requirementTokens?.Any() != true) return;

        if (!_authenticatedByBearer.IsAuthenticated()) return;

        AuthorizedClient userClient = await _authenticatedByBearer.GetAuthenticated();
        if (userClient.RefreshToken == null) return;

        List<Privilege> privileges = userClient.RefreshToken.TokenPrivileges.Privileges.ToList();
        foreach (string requirementToken in requirementTokens)
            if (privileges.FirstOrDefault<Privilege?>(p => p != null && p.Name == requirementToken && p.Value != null && (bool)p.Value == true, null) != null)
            {
                Utility.Succeed(context, requirement.Identifier);
                break;
            }

        return;
    }

}
