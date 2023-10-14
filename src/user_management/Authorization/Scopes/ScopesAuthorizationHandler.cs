namespace user_management.Authorization.Scopes;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

public class ScopesAuthorizationHandler : IAuthorizationHandler
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        return Task.CompletedTask;
    }
}