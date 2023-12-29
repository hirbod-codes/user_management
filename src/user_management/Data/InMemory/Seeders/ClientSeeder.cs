using Newtonsoft.Json;

namespace user_management.Data.InMemory.Seeders;

public class ClientSeeder : Data.Seeders.ClientSeeder
{
    private string? _filePath;
    private readonly InMemoryContext _inMemoryContext = null!;

    public ClientSeeder(InMemoryContext inMemoryContext, string? filePath)
    {
        if (filePath is not null)
            SetFilePath(filePath);
        _inMemoryContext = inMemoryContext;
    }

    public void SetFilePath(string rootPath)
    {
        var directoryPath = Path.Combine(rootPath, "Data/Seeders/Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "seeded_users.json");
    }

    public async Task Seed(int count)
    {
        System.Console.WriteLine("\nSeeding Clients...");

        IEnumerable<Models.Client> clients = GenerateClients(count, clients: _inMemoryContext.Clients);

        if (_filePath != null) await File.WriteAllTextAsync(_filePath!, JsonConvert.SerializeObject(clients));

        await PersistClients(clients);

        System.Console.WriteLine("Seeded Clients...\n");
    }

    private async Task PersistClients(IEnumerable<Models.Client> clients)
    {
        await _inMemoryContext.Clients.AddRangeAsync(clients);
        await _inMemoryContext.SaveChangesAsync();
    }

    public IEnumerable<Models.Client> GenerateClients(int count = 2, IEnumerable<Models.Client>? clients = null, DateTime? creationDateTime = null)
    {
        clients ??= Array.Empty<Models.Client>();
        if (creationDateTime == null) creationDateTime = DateTime.UtcNow;

        for (int i = 0; i < count; i++)
        {
            Models.Client? lastClient = clients.LastOrDefault();
            string clientId = lastClient is null ? "1" : (long.Parse(lastClient.Id) + 1).ToString();
            clients = clients.Append(FakeClient(clientId, out string secret, clients, creationDateTime));
        }
        return clients;
    }
}
