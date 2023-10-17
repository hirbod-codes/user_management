using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using user_management.Authorization.Attributes;

namespace user_management.Filters;

public class SecurityRequirementsFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.MethodInfo.GetCustomAttributes(true).OfType<AuthorizeAttribute>().Any())
            return;

        operation.Responses.Add("401", new() { Description = "Unauthenticated access detected." });

        if (context.MethodInfo.GetCustomAttributes(true).OfType<PermissionsAttribute>().Any())
            operation.Responses.Add("403", new() { Description = "Unauthorized access detected." });

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
