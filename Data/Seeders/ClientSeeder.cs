namespace user_management.Data.Seeders;

using System.Threading.Tasks;
using Bogus;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Models;
using user_management.Utilities;

public class ClientSeeder
{
    private readonly IMongoCollection<Client> _clientCollection;
    private readonly string _filePath;
    public ClientSeeder(MongoContext mongoContext, string rootPath)
    {
        MongoClient mongoClient = new MongoClient(mongoContext.ConnectionString);
        IMongoDatabase mongoDatabase = mongoClient.GetDatabase(mongoContext.DatabaseName);
        _clientCollection = mongoDatabase.GetCollection<Client>(mongoContext.Collections.Clients);

        var directoryPath = Path.Combine(rootPath, "Data\\Seeders\\Logs");
        Directory.CreateDirectory(directoryPath);
        _filePath = Path.Combine(directoryPath, "client_seeder_logs.log");
    }

    public async Task Seed()
    {
        await File.WriteAllTextAsync(_filePath, "");
        System.Console.WriteLine("Seeding Clients...");

        DateTime dt = DateTime.UtcNow;
        for (int i = 0; i < 5; i++)
        {
            await CreateClientAsync(dt);
            dt = dt.AddMinutes(10);
        }

        System.Console.WriteLine("Seeded Clients...");
    }

    private async Task CreateClientAsync(DateTime dt)
    {
        ObjectId id = ObjectId.GenerateNewId();

        Faker faker = new Faker("en");

        string secret;
        do
        {
            secret = (new StringHelper()).GenerateRandomString(128);

        } while ((await _clientCollection.FindAsync(Builders<Client>.Filter.Eq(Client.SECRET, (new StringHelper()).HashWithoutSalt(secret)))).FirstOrDefault<Client?>() != null);

        _clientCollection.InsertOne(new Client() { Id = id, Secret = (new StringHelper()).HashWithoutSalt(secret), RedirectUrl = faker.Internet.UrlWithPath(), CreatedAt = dt, UpdatedAt = dt });

        await File.AppendAllTextAsync(_filePath, @$"
Id ==> {id}
Secret ==> {secret}

");
    }
}