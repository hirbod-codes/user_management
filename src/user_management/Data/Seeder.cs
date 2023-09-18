namespace user_management.Data;

using user_management.Data.Seeders;

public class Seeder
{
    private readonly string? _rootPath;
    public MongoContext MongoContext { get; set; }

    public Seeder(MongoContext mongoContext, string? rootPath = null)
    {
        MongoContext = mongoContext;
        _rootPath = rootPath;
    }

    public async Task Seed()
    {
        System.Console.WriteLine("\nSeeding...");

        await ClientSeeder.Seed(MongoContext, _rootPath, count: 10);
        await UserSeeder.Seed(MongoContext, _rootPath, count: 50);

        System.Console.WriteLine("Seeded...\n");
    }
}
