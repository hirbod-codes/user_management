namespace user_management.Data.MongoDB.Seeders;

using user_management.Models;
using global::MongoDB.Driver;
using Newtonsoft.Json;
using global::MongoDB.Bson;

public class UserSeeder : Data.Seeders.UserSeeder
{
    private string? _filePath;
    private readonly IMongoCollection<User> _userCollection = null!;
    private readonly IMongoCollection<Client> _clientCollection = null!;
    public const string USERS_PASSWORDS = "Pass%w0rd!99";

    public UserSeeder(MongoCollections mongoCollections, string? rootPath)
    {
        if (rootPath is not null)
            SetFilePath(rootPath);

        _userCollection = mongoCollections.Users;
        _clientCollection = mongoCollections.Clients;
    }

    public void SetFilePath(string rootPath)
    {
        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public async Task Seed(int count = 2, FakeUserOptions? fakeUserOptions = null)
    {
        System.Console.WriteLine("\nSeeding Users...");

        IEnumerable<User> users = GenerateUsers(count, users: (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients: (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList(), fakeUserOptions);

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await PersistUsers(users);

        System.Console.WriteLine("Seeded Users...\n");
    }

    public IEnumerable<User> GenerateUsers(int count = 2, IEnumerable<User>? users = null, IEnumerable<Client>? clients = null, FakeUserOptions? fakeUserOptions = null)
    {
        users ??= Array.Empty<User>();
        for (int i = 0; i < count; i++)
        {
            string userId = ObjectId.GenerateNewId().ToString();
            users = users.Append(FakeUser(userId, users, clients, fakeUserOptions, password: USERS_PASSWORDS));
            if ((i + 1) % 10 == 0)
                System.Console.WriteLine($"Generated {i + 1} users\n");
        }
        return users;
    }

    public async Task PersistUsers(IEnumerable<User> users) => await _userCollection.InsertManyAsync(users);

    public async Task SeedAdmin(string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        System.Console.WriteLine("\nSeeding Admin User...");

        string adminId = ObjectId.GenerateNewId().ToString();
        IEnumerable<User> users = new List<User>() { GetAdminUser(adminId, adminUsername, adminPassword, adminEmail, adminPhoneNumber) };

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await PersistUsers(users);

        System.Console.WriteLine("Seeded Admin User...\n");
    }
}
