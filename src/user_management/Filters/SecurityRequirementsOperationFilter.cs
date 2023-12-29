using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using user_management.Authorization.Attributes;

namespace user_management.Filters;

public class SecurityRequirementsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.GetCustomAttributes(true).OfType<PermissionsAttribute>().Any())
        {
            if (!operation.Responses.TryGetValue("401", out OpenApiResponse? response401))
                operation.Responses.Add("401", new() { Description = "Unauthenticated access detected." });
            if (!operation.Responses.TryGetValue("403", out OpenApiResponse? response403))
                operation.Responses.Add("403", new() { Description = "Unauthorized access detected." });
        }
        else if (context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any() && !operation.Responses.TryGetValue("401", out OpenApiResponse? response401))
            operation.Responses.Add("401", new() { Description = "Unauthenticated access detected." });

        operation.Security.Add(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "user-auth" },
                },
                Array.Empty<string>()
            }
        });

        operation.Security.Add(new()
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "client-auth" },
                },
                Array.Empty<string>()
            }
        });
    }
}
