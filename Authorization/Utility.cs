namespace user_management.Authorization;

using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;

public static class Utility
{
    public static void Succeed(AuthorizationHandlerContext context, Guid identifier)
    {
        var groupedRequirements = context.Requirements.Where(r => (r as IIdentifiable)?.Identifier == identifier);

        foreach (var requirement in groupedRequirements)
        {
            context.Succeed(requirement);
        }
    }
}