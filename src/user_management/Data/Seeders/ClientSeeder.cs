namespace user_management.Data.Seeders;

using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using user_management.Models;

public static class ClientSeeder
{
    private static IMongoCollection<Client> _clientCollection = null!;
    private static string? _filePath;

    public static void Setup(MongoContext mongoContext, string? rootPath)
    {
        SetFilePath(rootPath);
        SetClientsCollection(mongoContext);
    }

    public static void SetFilePath(string? rootPath)
    {
        if (rootPath == null) return;

        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public static void SetClientsCollection(MongoContext mongoContext)
    {
        MongoClient mongoClient = mongoContext.GetMongoClient();
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.DatabaseName);
        _clientCollection = mongoDatabase.GetCollection<Client>(mongoContext.Collections.Clients);
    }

    public static async Task Seed(MongoContext mongoContext, string? rootPath, int count = 2)
    {
        if (_filePath == null || _clientCollection == null) Setup(mongoContext, rootPath);

        System.Console.WriteLine("\nSeeding Clients...");

        IEnumerable<Client> clients = GenerateClients(count);

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(clients));

        await PersistClients(clients);

        System.Console.WriteLine("Seeded Clients...\n");
    }

    private static async Task PersistClients(IEnumerable<Client> clients) => await _clientCollection.InsertManyAsync(clients);

    public static IEnumerable<Client> GenerateClients(int count = 2, IEnumerable<Client>? clients = null, DateTime? creationDateTime = null)
    {
        if (clients == null) clients = new Client[] { };
        if (creationDateTime == null) creationDateTime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
            clients = clients.Append(Client.FakeClient(clients, creationDateTime));
        return clients;
    }
}