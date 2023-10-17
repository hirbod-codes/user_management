namespace user_management.Data.Seeders;

using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json;
using user_management.Models;

public static class ClientSeeder
{
    private static IMongoCollection<Client> _clientCollection = null!;
    private static string? _filePath;

    public static void Setup(MongoCollections mongoCollections, string? rootPath)
    {
        SetFilePath(rootPath);
        SetClientsCollection(mongoCollections);
    }

    public static void SetFilePath(string? rootPath)
    {
        if (rootPath == null) return;

        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public static void SetClientsCollection(MongoCollections mongoCollections) => _clientCollection = mongoCollections.Clients;

    public static async Task Seed(MongoCollections mongoCollections, string? rootPath, int count = 2)
    {
        if (_filePath == null || _clientCollection == null) Setup(mongoCollections, rootPath);

        System.Console.WriteLine("\nSeeding Clients...");

        IEnumerable<Client> clients = GenerateClients(count, clients: (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());

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
            clients = clients.Append(Client.FakeClient(out string secret, clients, creationDateTime));
        return clients;
    }
}
