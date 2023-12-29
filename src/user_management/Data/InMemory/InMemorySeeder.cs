using user_management.Data.InMemory;
using user_management.Data.InMemory.Seeders;

namespace user_management.Data;

public class InMemorySeeder
{
    private readonly string _rootPath;
    private readonly InMemoryContext _inMemoryContext;

    public InMemorySeeder(string rootPath, InMemoryContext inMemoryContext)
    {
        _rootPath = rootPath;
        _inMemoryContext = inMemoryContext;
    }

    public async Task Seed()
    {
        System.Console.WriteLine("\nSeeding...");

        await new ClientSeeder(_inMemoryContext, _rootPath).Seed(count: 10);
        await new UserSeeder(_inMemoryContext, _rootPath).Seed(count: 50);

        System.Console.WriteLine("Seeded...\n");
    }

    public async Task SeedAdmin(string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        System.Console.WriteLine("\nSeeding Admin...");

        await new UserSeeder(_inMemoryContext, _rootPath).SeedAdmin(adminUsername, adminPassword, adminEmail, adminPhoneNumber);

        System.Console.WriteLine("Seeded Admin...\n");
    }
}
