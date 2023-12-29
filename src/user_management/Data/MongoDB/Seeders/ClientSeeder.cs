namespace user_management.Data.MongoDB.Seeders;

using System.Threading.Tasks;
using global::MongoDB.Bson;
using global::MongoDB.Driver;
using Newtonsoft.Json;
using user_management.Models;

public class ClientSeeder : Data.Seeders.ClientSeeder
{
    private readonly IMongoCollection<Client> _clientCollection = null!;
    private string? _filePath;

    public ClientSeeder(MongoCollections mongoCollections, string? filePath)
    {
        if (filePath is not null)
            SetFilePath(filePath);
        _clientCollection = mongoCollections.Clients;
    }

    public void SetFilePath(string? rootPath)
    {
        if (rootPath == null) return;

        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public async Task Seed(int count = 2)
    {
        System.Console.WriteLine("\nSeeding Clients...");

        IEnumerable<Client> clients = GenerateClients(count, clients: (await _clientCollection.FindAsync(Builders<Client>.Filter.Empty)).ToList());

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(clients));

        await PersistClients(clients);

        System.Console.WriteLine("Seeded Clients...\n");
    }

    private async Task PersistClients(IEnumerable<Client> clients) => await _clientCollection.InsertManyAsync(clients);

    public IEnumerable<Client> GenerateClients(int count = 2, IEnumerable<Client>? clients = null, DateTime? creationDateTime = null)
    {
        clients ??= Array.Empty<Client>();
        if (creationDateTime == null) creationDateTime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            string clientId = ObjectId.GenerateNewId().ToString();
            clients = clients.Append(FakeClient(clientId, out string secret, clients, creationDateTime));
        }
        return clients;
    }
}
