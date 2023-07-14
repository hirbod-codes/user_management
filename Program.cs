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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IStringHelper, StringHelper>();
builder.Services.AddSingleton<IAuthHelper, AuthHelper>();
builder.Services.AddSingleton<INotificationHelper, NotificationHelper>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
MongoContext mongoContext = new();
builder.Configuration.GetSection("MongoDB").Bind(mongoContext);
builder.Services.AddSingleton<IMongoClient>(MongoContext.GetMongoClient(mongoContext));
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IClientRepository, ClientRepository>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.Configure<JWTAuthenticationOptions>(builder.Configuration.GetSection("JWT"));
builder.Services.AddSingleton<JWTAuthenticationOptions>();
builder.Services.AddSingleton<IJWTAuthenticationHandler, JWTAuthenticationHandler>();

builder.Services.Configure<BearerAuthenticationOptions>(builder.Configuration.GetSection("Bearer"));
builder.Services.AddSingleton<BearerAuthenticationOptions>();
builder.Services.AddSingleton<BearerAuthenticationHandler>();

builder.Services.AddAuthentication(defaultScheme: "JWT")
    .AddScheme<JWTAuthenticationOptions, JWTAuthenticationHandler>("JWT", null)
    .AddScheme<BearerAuthenticationOptions, BearerAuthenticationHandler>("Bearer", "Bearer", null)
    ;

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionsPolicyProvider>();

builder.Services.AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, RolesAuthorizationHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, ScopesAuthorizationHandler>();
;

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    IWebHostEnvironment? env = app.Services.GetService<IWebHostEnvironment>();
    if (env == null)
        throw new Exception("Failed to resolve IWebHostEnvironment.");

    mongoContext = app.Services.GetService<IOptions<MongoContext>>()!.Value;
    var client = MongoContext.GetMongoClient(mongoContext);
    if (client.GetDatabase(mongoContext.DatabaseName).GetCollection<User>(mongoContext.Collections.Users).CountDocuments(Builders<User>.Filter.Empty) == 0)
    {
        await MongoContext.Initialize(app.Services.GetService<IOptions<MongoContext>>()!);
        await (new Seeder(app.Services.GetService<IOptions<MongoContext>>()!, app.Services.GetService<IUserRepository>()!, env.ContentRootPath)).Seed();
    }
    else
        System.Console.WriteLine("The database is already seeded.");
}

app.Run();
