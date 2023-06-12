namespace user_management.Authorization.Permissions;

using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using user_management.Data.User;
using user_management.Models;
using MongoDB.Bson;

public class PermissionsAuthorizationHandler : AuthorizationHandler<PermissionsRequirement>
{
    private readonly ILogger<PermissionsAuthorizationHandler> _logger;
    private readonly IUserRepository _userRepository;

    public PermissionsAuthorizationHandler(ILogger<PermissionsAuthorizationHandler> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
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
            return AuthorizeJWT(ObjectId.Parse(id), context, requirement);
        else 
            return AuthorizeBearer(id, context, requirement);
    }

    private async Task AuthorizeJWT(ObjectId userId, AuthorizationHandlerContext context, PermissionsRequirement requirement)
    {
        string[] requirementTokens = requirement.Permissions.Split("|", StringSplitOptions.RemoveEmptyEntries);
        if (requirementTokens?.Any() != true) return;

        User? user = await _userRepository.RetrieveByIdForAuthorization(userId);
        if (user == null) return;
        List<Privilege> privileges = user.Privileges!.ToList();
        if (privileges.Count == 0) return;

        foreach (string requirementToken in requirementTokens)
            if (privileges.FirstOrDefault<Privilege?>(p => p != null && p.Name == requirementToken && p.Value == true, null) != null)
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

        User? user = await _userRepository.RetrieveByTokenValue(tokenValue);
        if (user == null || user.Clients.Length == 0) return;
        List<UserClient> userClients = user.Clients.ToList();
        UserClient? userClient = userClients.FirstOrDefault<UserClient?>(uc => uc != null, null);
        if (userClient == null) return;

        Utility.Succeed(context, requirement.Identifier);
        return;
    }

}