using MongoDB.Driver;

namespace user_management.Data;

public static class DatabaseManagement
{
    public static void ResolveDatabase(WebApplicationBuilder builder)
    {
        if (Environment.GetEnvironmentVariable("DB_NAME") == "mongodb" && Environment.GetEnvironmentVariable("DB_OPTIONS__IsSharded") == "true")
            builder.ConfigureShardedMongodb();
        else if (Environment.GetEnvironmentVariable("DB_NAME") == "mongodb" && Environment.GetEnvironmentVariable("DB_OPTIONS__IsSharded") != "true")
            builder.ConfigureMongodb();
    }

    public static async Task InitializeDatabase(WebApplication app)
    {
        if (
            Environment.GetEnvironmentVariable("DB_NAME") == "mongodb"
            && Environment.GetEnvironmentVariable("DB_OPTIONS__IsSharded") == "true"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(Environment.GetEnvironmentVariable("DB_OPTIONS__DatabaseName")!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await app.Services.GetService<ShardedMongoContext>()!.Initialize(app.Services.GetService<MongoCollections>()!, app.Services.GetService<IMongoDatabase>()!);
        else if (
            Environment.GetEnvironmentVariable("DB_NAME") == "mongodb"
            && Environment.GetEnvironmentVariable("DB_OPTIONS__IsSharded") != "true"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(Environment.GetEnvironmentVariable("DB_OPTIONS__DatabaseName")!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await app.Services.GetService<MongoContext>()!.Initialize(app.Services.GetService<MongoCollections>()!, app.Services.GetService<IMongoDatabase>()!);
    }

    public static async Task SeedDatabase(WebApplication app)
    {
        if (
            app.Environment.IsEnvironment("Development")
            && Environment.GetEnvironmentVariable("DB_NAME") == "mongodb"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(Environment.GetEnvironmentVariable("DB_OPTIONS__DatabaseName")!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await new MongoSeeder(app.Services.GetService<MongoCollections>()!, app.Environment.ContentRootPath).Seed();
        else if (
            app.Environment.IsEnvironment("IntegrationTest")
            && Environment.GetEnvironmentVariable("DB_NAME") == "mongodb"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(Environment.GetEnvironmentVariable("DB_OPTIONS__DatabaseName")!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await new MongoSeeder(app.Services.GetService<MongoCollections>()!, app.Environment.ContentRootPath).Seed();
    }
}
