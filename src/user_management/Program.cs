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
using DotNetEnv.Configuration;
using MongoDB.Driver;
using user_management.Configuration.Providers.DockerSecrets;
using Microsoft.OpenApi.Models;
using System.Reflection;
using Swashbuckle.AspNetCore.Filters;
using user_management.Filters;
using user_management.Notification;
using System.Net;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using user_management.Docs;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

Program.RootPath = builder.Environment.ContentRootPath;

builder.Configuration.AddEnvironmentVariables(Program.ENV_PREFIX);

if (builder.Environment.IsProduction())
    builder.Configuration.AddDockerSecrets(allowedPrefixesCommaDelimited: builder.Configuration["SECRETS_PREFIX"]);
else if (builder.Configuration["MUST_NOT_USE_ENV_FILE"] != "true" && builder.Configuration["MUST_NOT_USE_ENV_FILE"] != "true")
    builder.Configuration.AddDotNetEnv(Program.RootPath + "/../../" + (builder.Configuration["ENV_FILE_PATH"] ?? ".env.mongodb.development"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

if (!builder.Configuration["FirstPartyDomains"].IsNullOrEmpty())
    Program.FirstPartyDomains = Program.FirstPartyDomains.Concat<string>(builder.Configuration["FirstPartyDomains"]!.Split(",", StringSplitOptions.TrimEntries)).ToArray();

builder.Services.AddCors(opt =>
{
    opt.DefaultPolicyName = "default-cors-policy";
    opt.AddDefaultPolicy(c => { c.AllowAnyHeader(); c.AllowAnyMethod(); c.AllowAnyOrigin(); });
    opt.AddPolicy("third-party-clients", c => { c.AllowAnyHeader(); c.AllowAnyMethod(); c.WithOrigins(Program.FirstPartyDomains); });
    opt.AddPolicy("login", c => { c.AllowAnyHeader(); c.AllowAnyMethod(); c.WithOrigins(Program.FirstPartyDomains); });
});

builder.Services.AddApiVersioning(opt =>
{
    opt.AssumeDefaultVersionWhenUnspecified = true;
    opt.DefaultApiVersion = ApiVersion.Parse("1.0");
    opt.ReportApiVersions = true;
    opt.ApiVersionReader = ApiVersionReader.Combine(
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version")
    );
});

builder.Services.AddVersionedApiExplorer(o =>
{
    o.GroupNameFormat = "'v'VVV";
    o.SubstituteApiVersionInUrl = true;
});

builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(c =>
{
    c.ExampleFilters();
    c.IncludeXmlComments(Path.Combine(System.AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), true);

    c.AddSecurityDefinition("user-auth", new OpenApiSecurityScheme
    {
        Name = "user-auth",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "A jwt token for user authentication"
    });

    c.AddSecurityDefinition("client-auth", new OpenApiSecurityScheme
    {
        Name = "client-auth",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        Description = "A token for client authentication"
    });

    c.OperationFilter<SecurityRequirementsFilter>();
    c.OperationFilter<GlobalResponsesFilter>();

    c.OperationFilter<SwaggerDefaultValues>();
});
builder.Services.AddSwaggerGenNewtonsoftSupport();
builder.Services.AddSwaggerExamplesFromAssemblyOf<Program>();

NotificationOptions notificationOptions = new();
builder.Configuration.GetSection("NOTIFICATION_OPTIONS").Bind(notificationOptions);
builder.Services.AddSingleton(notificationOptions);
builder.Services.AddSingleton<INotificationProvider, NotificationProvider>();

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
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DocumentTitle = "User Management - docs";
    c.RoutePrefix = "swagger";
    c.EnableDeepLinking();
    c.DefaultModelsExpandDepth(0);

    foreach (var description in app.Services.GetRequiredService<IApiVersionDescriptionProvider>().ApiVersionDescriptions)
        c.SwaggerEndpoint($"{description.GroupName}/swagger.json", $"{description.GroupName}");
});

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

await DatabaseManagement.SeedDatabase(
    app.Services.GetService<IMongoClient>()!,
    app.Services.GetService<MongoCollections>()!,
    app.Configuration["ENVIRONMENT"]!,
    app.Configuration["DB_NAME"]!,
    app.Configuration["DB_OPTIONS:DatabaseName"]!,
    app.Configuration["ADMIN_USERNAME"]!,
    app.Configuration["ADMIN_PASSWORD"]!,
    app.Configuration["ADMIN_EMAIL"]!,
    app.Configuration["ADMIN_PHONE_NUMBER"]
    );

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseCors("default-cors-policy");
app.UseCors("third-party-clients");
app.UseCors("login");

app.UseAuthorization();

app.Use((context, next) =>
{
    if (context.Request.Path.Value != "/api/" + user_management.Controllers.UserController.PATH_POST_LOGIN && context.Request.Path.Value != "/api/" + user_management.Controllers.TokenController.PATH_POST_AUTHORIZE)
        return next();

    if (Program.FirstPartyDomains.Contains(context.Request.Host.Host.ToLower()))
        return next();

    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
    return Task.FromResult<object?>(null);
});

app.MapControllers();

if (!app.Environment.IsDevelopment())
{
    app.UseDatabaseExceptionHandler();
    app.UseExceptionHandler(handler => handler.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        // using static System.Net.Mime.MediaTypeNames;
        context.Response.ContentType = Text.Plain;

        await context.Response.WriteAsync("An exception was thrown.");
    }));
}

app.Run();

public partial class Program
{
    public const string ENV_PREFIX = "USER_MANAGEMENT_";
    public static string RootPath { get; set; } = "";

    public static string[] FirstPartyDomains { get; set; } = new string[] { "::1", "127.0.0.1", "0.0.0.0", "localhost" };

    public void Configure() { }
}
