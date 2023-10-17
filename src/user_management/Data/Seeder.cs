namespace user_management.Data;

using user_management.Data.Seeders;

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

        await ClientSeeder.Seed(_mongoCollections, _rootPath, count: 10);
        await UserSeeder.Seed(_mongoCollections, _rootPath, count: 50);

        System.Console.WriteLine("Seeded...\n");
    }

    public async  Task SeedAdmin(string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        System.Console.WriteLine("\nSeeding Admin...");

        await UserSeeder.SeedAdmin(_mongoCollections, _rootPath, adminUsername, adminPassword, adminEmail, adminPhoneNumber);

        System.Console.WriteLine("Seeded Admin...\n");
    }
}
