namespace user_management.Data.Seeders;

using user_management.Models;
using MongoDB.Driver;
using Newtonsoft.Json;
using user_management.Utilities;

public static class UserSeeder
{
    private static string? _filePath;
    private static IMongoCollection<User> _userCollection = null!;
    private static IMongoCollection<Client> _clientCollection = null!;
    public const string USERS_PASSWORDS = "Pass%w0rd!99";

    public static void Setup(MongoCollections mongoCollections, string? rootPath)
    {
        SetFilePath(rootPath);
        SetClientsCollection(mongoCollections);
        SetUsersCollection(mongoCollections);
    }

    public static void SetFilePath(string? rootPath)
    {
        if (rootPath == null) return;

        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public static void SetUsersCollection(MongoCollections mongoCollections) => _userCollection = mongoCollections.Users;

    public static void SetClientsCollection(MongoCollections mongoCollections) => _clientCollection = mongoCollections.Clients;

    public static async Task Seed(MongoCollections mongoCollections, string? rootPath, FakeUserOptions? fakeUserOptions = null, int count = 2)
    {
        if (_filePath == null || _userCollection == null || _clientCollection == null) Setup(mongoCollections, rootPath);

        System.Console.WriteLine("\nSeeding Users...");

        IEnumerable<User> users = GenerateUsers(count, users: (await _userCollection.FindAsync(Builders<User>.Filter.Empty)).ToList(), clients: (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await PersistUsers(users);

        System.Console.WriteLine("Seeded Users...\n");
    }

    public static IEnumerable<User> GenerateUsers(int count = 2, IEnumerable<User>? users = null, IEnumerable<Client>? clients = null)
    {
        users ??= Array.Empty<User>();
        for (int i = 0; i < count; i++)
        {
            users = users.Append(User.FakeUser(users, clients, new FakeUserOptions()));
            if ((i + 1) % 10 == 0) System.Console.WriteLine($"Generated {i + 1} users\n");
        }
        return users;
    }

    public static async Task PersistUsers(IEnumerable<User> users) => await _userCollection.InsertManyAsync(users);

    public static async Task SeedAdmin(MongoCollections mongoCollections, string? rootPath, string adminUsername, string adminPassword, string adminEmail, string? adminPhoneNumber)
    {
        if (_filePath == null || _userCollection == null || _clientCollection == null) Setup(mongoCollections, rootPath);

        System.Console.WriteLine("\nSeeding Admin User...");


        IEnumerable<User> users = new List<User>() { User.GetAdminUser(adminUsername, adminPassword, adminEmail, adminPhoneNumber) };

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await PersistUsers(users);

        System.Console.WriteLine("Seeded Admin User...\n");
    }
}
