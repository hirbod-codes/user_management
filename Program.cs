using Microsoft.Extensions.Options;
using user_management.Data;
using user_management.Authentication.JWT;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<MongoContext>(builder.Configuration.GetSection("MongoDB"));
builder.Services.AddSingleton<IMongoClient>(new MongoClient(builder.Configuration.GetSection("MongoDB").GetValue<string>("ConnectionString")));
builder.Services.Configure<JWTAuthenticationOptions>(builder.Configuration.GetSection("JWT"));
builder.Services.AddSingleton<JWTAuthenticationOptions>();
builder.Services.AddSingleton<IJWTAuthenticationHandler, JWTAuthenticationHandler>();
builder.Services.AddAuthentication(defaultScheme: "JWT")
    .AddScheme<JWTAuthenticationOptions, JWTAuthenticationHandler>("JWT", null)
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
