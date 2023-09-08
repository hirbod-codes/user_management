using Microsoft.Extensions.Options;
using user_management.Data;
using user_management.Data.User;
using user_management.Data.Client;
using user_management.Authentication.JWT;
using user_management.Authentication.Bearer;
using Microsoft.AspNetCore.Authorization;
using user_management.Authorization;
using user_management.Authorization.Permissions;
using user_management.Authorization.Roles;
using user_management.Authorization.Scopes;
using user_management.Utilities;
using MongoDB.Driver;
using user_management.Models;
using user_management.Middlewares;
using static System.Net.Mime.MediaTypeNames;
using user_management.Services;
using user_management.Services.Data.User;
using user_management.Services.Client;
using user_management.Controllers.Services;
using user_management.Authentication;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
MongoContext mongoContext = new();
builder.Configuration.GetSection("MongoDB").Bind(mongoContext);
builder.Services.AddSingleton<MongoContext>(mongoContext);
builder.Services.AddSingleton<IMongoClient>(mongoContext.GetMongoClient());
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IClientRepository, ClientRepository>();

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

if (app.Environment.IsDevelopment())
{
    IWebHostEnvironment? env = app.Services.GetService<IWebHostEnvironment>();
    if (env == null)
        throw new Exception("Failed to resolve IWebHostEnvironment.");

    mongoContext = app.Services.GetService<IOptions<MongoContext>>()!.Value;
    var client = mongoContext.GetMongoClient();
    if (client.GetDatabase(mongoContext.DatabaseName).GetCollection<User>(mongoContext.Collections.Users).CountDocuments(Builders<User>.Filter.Empty) == 0)
    {
        await mongoContext.Initialize();
        await (new Seeder(app.Services.GetService<MongoContext>()!, env.ContentRootPath)).Seed();
    }
    else
        System.Console.WriteLine("The database is already seeded.");
}

app.UseDatabaseExceptionHandler();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

    // using static System.Net.Mime.MediaTypeNames;
    context.Response.ContentType = Text.Plain;

    await context.Response.WriteAsync("An exception was thrown.");
}));

app.Run();