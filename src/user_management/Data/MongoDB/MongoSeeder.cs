namespace user_management.Data.MongoDB;

using user_management.Data.MongoDB.Seeders;

public class MongoSeeder
{
    private readonly string? _rootPath;
    private MongoCollections _mongoCollections;

    public MongoSeeder(MongoCollections mongoCollections, string? rootPath = null)
    {
        _mongoCollections = mongoCollections;
        _rootPath = rootPath;
    }

    public async Task Seed()
    {
        System.Console.WriteLine("\nSeeding...");

        await new ClientSeeder(_mongoCollections, _rootPath).Seed(count: 10);
        await new UserSeeder(_mongoCollections, _rootPath).Seed(count: 50);

        System.Console.WriteLine("Seeded...\n");
    }

    public async Task SeedAdmin(string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        System.Console.WriteLine("\nSeeding Admin...");

        await new UserSeeder(_mongoCollections, _rootPath).SeedAdmin(adminUsername, adminPassword, adminEmail, adminPhoneNumber);

        System.Console.WriteLine("Seeded Admin...\n");
    }
}
