using user_management.Data;
using user_management.Authentication.JWT;
using user_management.Authentication.Bearer;
using Microsoft.AspNetCore.Authorization;
using user_management.Authorization;
using user_management.Authorization.Permissions;
using user_management.Authorization.Roles;
using user_management.Authorization.Scopes;
using user_management.Utilities;
using user_management.Middlewares;
using static System.Net.Mime.MediaTypeNames;
using user_management.Services;
using user_management.Controllers.Services;
using user_management.Authentication;
using DotNetEnv;
using DotNetEnv.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

if (builder.Configuration.GetSection("SHOULD_NOT_USE_ENV_FILE").Value != "true")
    builder.Configuration.AddDotNetEnv("../../.env.mongodb.development", LoadOptions.TraversePath());

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IStringHelper, StringHelper>();
builder.Services.AddSingleton<IAuthHelper, AuthHelper>();
builder.Services.AddSingleton<INotificationHelper, NotificationHelper>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

builder.Services.AddScoped<IUserManagement, UserManagement>();
builder.Services.AddScoped<IUserPrivilegesManagement, UserPrivilegesManagement>();
builder.Services.AddScoped<IClientManagement, ClientManagement>();
builder.Services.AddScoped<ITokenManagement, TokenManagement>();

DatabaseManagement.ResolveDatabase(builder);

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<JWTAuthenticationOptions>(builder.Configuration.GetSection("JWT"));
builder.Services.AddSingleton<JWTAuthenticationOptions>();
builder.Services.AddScoped<JWTAuthenticationHandler>();

builder.Services.Configure<BearerAuthenticationOptions>(builder.Configuration.GetSection("Bearer"));
builder.Services.AddSingleton<BearerAuthenticationOptions>();
builder.Services.AddScoped<BearerAuthenticationHandler>();

builder.Services.AddAuthentication(defaultScheme: "JWT")
    .AddScheme<JWTAuthenticationOptions, JWTAuthenticationHandler>("JWT", null)
    .AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>("Bearer", "Bearer", null)
    ;

builder.Services.AddScoped<IAuthenticated, Authenticated>();
builder.Services.AddScoped<IAuthenticatedByJwt, AuthenticatedByJwt>();
builder.Services.AddScoped<IAuthenticatedByBearer, AuthenticatedByBearer>();

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionsPolicyProvider>();

builder.Services.AddScoped<IAuthorizationHandler, PermissionsAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, RolesAuthorizationHandler>();
builder.Services.AddScoped<IAuthorizationHandler, ScopesAuthorizationHandler>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors(cpb =>
{
    cpb.AllowAnyHeader();
    cpb.AllowAnyMethod();
    cpb.AllowAnyOrigin();
});

app.UseAuthorization();

app.MapControllers();

await DatabaseManagement.InitializeDatabase(app);

await DatabaseManagement.SeedDatabase(app);

app.UseDatabaseExceptionHandler();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

    // using static System.Net.Mime.MediaTypeNames;
    context.Response.ContentType = Text.Plain;

    await context.Response.WriteAsync("An exception was thrown.");
}));

app.Run();

public partial class Program
{
    public void Configure() { }
}
