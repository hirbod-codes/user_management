using Microsoft.Extensions.Options;
using user_management.Data;
using user_management.Data.User;
using user_management.Authentication.JWT;
using user_management.Authentication.Bearer;
using Microsoft.AspNetCore.Authorization;
using user_management.Authorization;
using user_management.Authorization.Permissions;
using user_management.Authorization.Roles;
using user_management.Authorization.Scopes;
using user_management.Utilities;
using MongoDB.Driver;

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
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetSection("MongoDB").GetValue<string>("ConnectionString")));
builder.Services.AddSingleton<IUserRepository, UserRepository>();
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

    string filePath = Path.Combine(env.ContentRootPath, "appSettings.json");
    string json = File.ReadAllText(filePath);
    dynamic jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(json)!;

    if (jsonObj["MongoDB"]["IsSeeded"] == false)
    {
        await MongoContext.Initialize(app.Services.GetService<IOptions<MongoContext>>()!);
    }
    else
        System.Console.WriteLine("The database is already seeded.");

    jsonObj["MongoDB"]["IsSeeded"] = true;

    string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
    File.WriteAllText(filePath, output);
}

app.Run();
