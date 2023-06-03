namespace user_management.Data;

using Microsoft.Extensions.Options;
using user_management.Data.Seeders;
using user_management.Data.User;

public class Seeder
{
    private readonly IUserRepository _userRepository;
    private readonly string _rootPath;

    public MongoContext MongoContext { get; set; }

    public Seeder(IOptions<MongoContext> mongoContext, IUserRepository userRepository, string rootPath)
    {
        MongoContext = mongoContext.Value;
        _userRepository = userRepository;
        _rootPath = rootPath;
    }

    public async Task Seed()
    {
        System.Console.WriteLine("Seeding...");

        await (new ClientSeeder(MongoContext, _rootPath)).Seed();
        await (new UserSeeder(MongoContext, _rootPath)).Seed();

        System.Console.WriteLine("Seeded...");
    }
}