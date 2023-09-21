namespace user_management.Data;

using user_management.Data.Seeders;

public class Seeder
{
    private readonly string? _rootPath;
    public ShardedMongoContext ShardedMongoContext { get; set; }

    public Seeder(ShardedMongoContext mongoContext, string? rootPath = null)
    {
        ShardedMongoContext = mongoContext;
        _rootPath = rootPath;
    }

    public async Task Seed()
    {
        System.Console.WriteLine("\nSeeding...");

        await ClientSeeder.Seed(ShardedMongoContext, _rootPath, count: 10);
        await UserSeeder.Seed(ShardedMongoContext, _rootPath, count: 50);

        System.Console.WriteLine("Seeded...\n");
    }
}
