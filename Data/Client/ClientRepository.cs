namespace user_management.Data.Client;

using Microsoft.Extensions.Options;
using MongoDB.Driver;
using user_management.Models;
using user_management.Data;
using MongoDB.Bson;

public class ClientRepository : IClientRepository
{
    private readonly IMongoCollection<Client> _clientCollection;

    public ClientRepository(IOptions<MongoContext> MongoContext)
    {
        var mongoClient = new MongoClient(MongoContext.Value.ConnectionString);

        var mongoDatabase = mongoClient.GetDatabase(MongoContext.Value.DatabaseName);

        _clientCollection = mongoDatabase.GetCollection<Client>(MongoContext.Value.Collections.Clients);
    }

    public async Task<Client> Create(Client client)
    {
        client.Id = ObjectId.GenerateNewId();

        DateTime dt = DateTime.UtcNow;
        client.UpdatedAt = dt;
        client.CreatedAt = dt;

        await _clientCollection.InsertOneAsync(client);

        return client;
    }
}