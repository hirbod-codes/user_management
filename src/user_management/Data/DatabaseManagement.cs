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
        else throw new MissingDatabaseConfiguration();
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

    public static async Task SeedDatabase(IMongoClient client, MongoCollections mongoCollections, string environment, string dbName, string dbOptionsDatabaseName, string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        if (
            environment == "Development"
            && dbName == "mongodb"
            && (
                client.GetDatabase(dbOptionsDatabaseName).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
                || client.GetDatabase(dbOptionsDatabaseName).GetCollection<user_management.Models.Client>(MongoCollections.CLIENTS).EstimatedDocumentCount() == 0
            )
        )
        {
            await mongoCollections.ClearCollections(client.GetDatabase(dbOptionsDatabaseName));
            await new MongoSeeder(mongoCollections, Program.RootPath).Seed();

            // Add admin user
            await new MongoSeeder(mongoCollections, Program.RootPath).SeedAdmin(adminUsername, adminPassword, adminEmail, adminPhoneNumber);
        }
        else if (
            environment == "IntegrationTest"
            && dbName == "mongodb"
            && (
                client.GetDatabase(dbOptionsDatabaseName).GetCollection<user_management.Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
                || client.GetDatabase(dbOptionsDatabaseName).GetCollection<user_management.Models.Client>(MongoCollections.CLIENTS).EstimatedDocumentCount() == 0
            )
        )
        {
            await mongoCollections.ClearCollections(client.GetDatabase(dbOptionsDatabaseName));
            await new MongoSeeder(mongoCollections, Program.RootPath).Seed();
        }
    }
}
