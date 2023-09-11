namespace user_management.Data.Seeders;

using user_management.Models;
using MongoDB.Driver;
using Newtonsoft.Json;

public static class UserSeeder
{
    private static string? _filePath;
    private static IMongoCollection<User> _userCollection = null!;
    private static IMongoCollection<Client> _clientCollection = null!;

    public static void Setup(MongoContext mongoContext, string? rootPath)
    {
        SetFilePath(rootPath);
        SetClientsCollection(mongoContext);
        SetUsersCollection(mongoContext);
    }

    public static void SetFilePath(string? rootPath)
    {
        if (rootPath == null) return;

        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public static void SetUsersCollection(MongoContext mongoContext)
    {
        MongoClient mongoClient = mongoContext.GetMongoClient();
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.DatabaseName);
        _userCollection = mongoDatabase.GetCollection<User>(mongoContext.Collections.Users);
    }

    public static void SetClientsCollection(MongoContext mongoContext)
    {
        MongoClient mongoClient = mongoContext.GetMongoClient();
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.DatabaseName);
        _clientCollection = mongoDatabase.GetCollection<Client>(mongoContext.Collections.Clients);
    }

    public static async Task Seed(MongoContext mongoContext, string? rootPath, FakeUserOptions? fakeUserOptions = null, int count = 2)
    {
        if (_filePath == null || _userCollection == null || _clientCollection == null) Setup(mongoContext, rootPath);

        System.Console.WriteLine("\nSeeding Users...");

        IEnumerable<User> users = GenerateUsers(count, clients: (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(users));

        await PersistUsers(users);

        System.Console.WriteLine("Seeded Users...\n");
    }

    public static IEnumerable<User> GenerateUsers(int count = 2, IEnumerable<User>? users = null, IEnumerable<Client>? clients = null)
    {
        if (users == null) users = new User[] { };
        for (int i = 0; i < count; i++)
        {
            users = users.Append(User.FakeUser(users, clients, new FakeUserOptions()));
            if ((i + 1) % 10 == 0) System.Console.WriteLine($"Generated {i + 1} users\n");
        }
        return users;
    }

    public static async Task PersistUsers(IEnumerable<User> users) => await _userCollection.InsertManyAsync(users);
}