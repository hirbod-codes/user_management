using MongoDB.Driver;
using user_management.Data.InMemory;
using user_management.Data.MongoDB;

namespace user_management.Data;

public static class DatabaseManagement
{
    public static void ResolveDatabase(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration["DB_NAME"]! == "mongodb" && configuration["DB_OPTIONS:IsSharded"]! == "true")
            ShardedMongoContext.ConfigureShardedMongodb(services, configuration);
        else if (configuration["DB_NAME"]! == "mongodb" && configuration["DB_OPTIONS:IsSharded"]! != "true")
            MongoContext.ConfigureMongodb(services, configuration);
        else if (configuration["DB_NAME"]! == "in_memory")
            InMemoryContext.ConfigureInMemory(services, configuration);
        else throw new MissingDatabaseConfiguration();
    }

    public static async Task InitializeDatabase(IServiceProvider services, IConfiguration configuration)
    {
        if (
            configuration["DB_NAME"]! == "mongodb"
            && configuration["DB_OPTIONS:IsSharded"]! == "true"
            && services.GetService<IMongoClient>()!.GetDatabase(configuration["DB_OPTIONS:DatabaseName"]!).GetCollection<Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await services.GetService<ShardedMongoContext>()!.Initialize(services.GetService<MongoCollections>()!, services.GetService<IMongoDatabase>()!);
        else if (
            configuration["DB_NAME"]! == "mongodb"
            && configuration["DB_OPTIONS:IsSharded"]! != "true"
            && services.GetService<IMongoClient>()!.GetDatabase(configuration["DB_OPTIONS:DatabaseName"]!).GetCollection<Models.User>(MongoCollections.USERS).EstimatedDocumentCount() == 0
        )
            await services.GetService<MongoContext>()!.Initialize(services.GetService<MongoCollections>()!, services.GetService<IMongoDatabase>()!);
    }

    public static async Task SeedDatabase(IServiceProvider services, IConfiguration configuration)
    {
        if (configuration["DB_NAME"]! == "mongodb")
        {
            IMongoClient client = services.GetRequiredService<IMongoClient>();
            MongoCollections mongoCollections = services.GetRequiredService<MongoCollections>();

            if (
                (configuration["ENVIRONMENT"]! == "Development" || configuration["ENVIRONMENT"]! == "IntegrationTest")
                && (
                    await client.GetDatabase(configuration["DB_OPTIONS:DatabaseName"]!).GetCollection<Models.User>(MongoCollections.USERS).EstimatedDocumentCountAsync() == 0
                    || await client.GetDatabase(configuration["DB_OPTIONS:DatabaseName"]!).GetCollection<Models.Client>(MongoCollections.CLIENTS).EstimatedDocumentCountAsync() == 0
                )
            )
            {
                await mongoCollections.ClearCollections(client.GetDatabase(configuration["DB_OPTIONS:DatabaseName"]!));
                await new MongoSeeder(mongoCollections, Program.RootPath).Seed();

                await new MongoSeeder(mongoCollections, Program.RootPath).SeedAdmin(configuration["ADMIN_USERNAME"]!, configuration["ADMIN_PASSWORD"]!, configuration["ADMIN_EMAIL"]!, configuration["ADMIN_PHONE_NUMBER"]!);
            }
        }
        else if (configuration["DB_NAME"]! == "in_memory")
        {
            InMemoryContext inMemoryContext = services.GetRequiredService<InMemoryContext>();
            if (
                (configuration["ENVIRONMENT"]! == "Development" || configuration["ENVIRONMENT"]! == "IntegrationTest")
                && (!inMemoryContext.Users.Any()
                    || !inMemoryContext.Clients.Any())
            )
            {
                await inMemoryContext.ClearDatabase();
                await new InMemorySeeder(Program.RootPath, inMemoryContext).Seed();

                await new InMemorySeeder(Program.RootPath, inMemoryContext).SeedAdmin(configuration["ADMIN_USERNAME"]!, configuration["ADMIN_PASSWORD"]!, configuration["ADMIN_EMAIL"]!, configuration["ADMIN_PHONE_NUMBER"]!);
            }
        }
    }
}
