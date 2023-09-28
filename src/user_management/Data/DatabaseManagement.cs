using MongoDB.Driver;

namespace user_management.Data;

public static class DatabaseManagement
{
    public static void ResolveDatabase(WebApplicationBuilder builder)
    {
        if (builder.Configuration.GetSection("DB_NAME").Value! == "mongodb" && builder.Configuration.GetSection("DB_OPTIONS:IsSharded").Value == "true")
            builder.ConfigureShardedMongodb();
        else if (builder.Configuration.GetSection("DB_NAME").Value! == "mongodb" && builder.Configuration.GetSection("DB_OPTIONS:IsSharded").Value != "true")
            builder.ConfigureMongodb();
    }

    public static async Task InitializeDatabase(WebApplication app)
    {
        if (
            app.Configuration.GetSection("DB_NAME").Value! == "mongodb"
            && app.Configuration.GetSection("DB_OPTIONS:IsSharded").Value == "true"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(app.Configuration.GetSection("DB_OPTIONS:DatabaseName").Value!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await app.Services.GetService<ShardedMongoContext>()!.Initialize(app.Services.GetService<MongoCollections>()!, app.Services.GetService<IMongoDatabase>()!);
        else if (
            app.Configuration.GetSection("DB_NAME").Value! == "mongodb"
            && app.Configuration.GetSection("DB_OPTIONS:IsSharded").Value != "true"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(app.Configuration.GetSection("DB_OPTIONS:DatabaseName").Value!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await app.Services.GetService<MongoContext>()!.Initialize(app.Services.GetService<MongoCollections>()!, app.Services.GetService<IMongoDatabase>()!);
    }

    public static async Task SeedDatabase(WebApplication app)
    {
        if (
            app.Environment.IsEnvironment("Development")
            && app.Configuration.GetSection("DB_NAME").Value! == "mongodb"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(app.Configuration.GetSection("DB_OPTIONS__DatabaseName").Value!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await new MongoSeeder(app.Services.GetService<MongoCollections>()!, app.Environment.ContentRootPath).Seed();
        else if (
            app.Environment.IsEnvironment("IntegrationTest")
            && app.Configuration.GetSection("DB_NAME").Value! == "mongodb"
            && app.Services.GetService<IMongoClient>()!.GetDatabase(app.Configuration.GetSection("DB_OPTIONS__DatabaseName").Value!).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await new MongoSeeder(app.Services.GetService<MongoCollections>()!, app.Environment.ContentRootPath).Seed();
    }
}
