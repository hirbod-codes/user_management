namespace user_management.Authorization.Roles;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public class RolesAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        return Task.CompletedTask;
    }
}