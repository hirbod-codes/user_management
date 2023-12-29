using Microsoft.EntityFrameworkCore;
using user_management.Services.Data.Client;

namespace user_management.Data.InMemory.Client;

public class ClientRepository : InMemoryAtomicity, IClientRepository
{
    public ClientRepository(InMemoryContext inMemoryContext) : base(inMemoryContext) { }

    public Task<bool?> ClientExposed(string clientId, string hashedSecret, string newHashedSecret)
    {
        throw new NotImplementedException();
    }

    public async Task<bool?> ClientExposed(Models.Client client, string newHashedSecret)
    {
        await DeleteBySecret(client.Secret);

        client.Secret = newHashedSecret;
        client.ExposedCount++;
        client.TokensExposedAt = DateTime.UtcNow;
        client.UpdatedAt = DateTime.UtcNow;
        client.CreatedAt = DateTime.UtcNow;

        await Create(client);
        return true;
    }

    public Task<bool?> ClientExposedWithTransaction(string clientId, string hashedSecret, string newHashedSecret) => ClientExposed(clientId, hashedSecret, newHashedSecret);

    public Task<bool?> ClientExposedWithTransaction(Models.Client client, string newHashedSecret) => ClientExposed(client, newHashedSecret);

    public async Task<Models.Client> Create(Models.Client client)
    {
        InMemoryContext.Add(client);
        await InMemoryContext.SaveChangesAsync();
        return client;
    }

    public Task<Models.Client> CreateWithTransaction(Models.Client client) => Create(client);

    public async Task<bool> DeleteById(string clientId)
    {
        Models.Client? client = await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Id == clientId);
        if (client is null)
            return false;
        InMemoryContext.Clients.Remove(client);
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> DeleteByIdWithTransaction(string clientId) => DeleteById(clientId);

    public async Task<bool> DeleteBySecret(string hashedSecret)
    {
        Models.Client? client = await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Secret == hashedSecret);
        if (client is null)
            return false;
        InMemoryContext.Clients.Remove(client);
        await InMemoryContext.SaveChangesAsync();
        return true;
    }

    public Task<bool> DeleteBySecretWithTransaction(string hashedSecret) => DeleteBySecret(hashedSecret);

    public int GetEstimatedDocumentCount() => InMemoryContext.Clients.Count();

    public async Task<Models.Client?> RetrieveById(string clientId) => await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Id == clientId);

    public async Task<Models.Client?> RetrieveByIdAndRedirectUrl(string clientId, string redirectUrl) => await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Id == clientId && o.RedirectUrl == redirectUrl);

    public async Task<Models.Client?> RetrieveByIdAndSecret(string clientId, string hashedSecret) => await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Id == clientId && o.Secret == hashedSecret);

    public async Task<Models.Client?> RetrieveBySecret(string hashedSecret) => await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Secret == hashedSecret);

    public Task<IEnumerable<Models.Client>> RetrieveFirstPartyClients() => Task.FromResult<IEnumerable<Models.Client>>(InMemoryContext.Clients.Where(o => o.IsFirstParty).ToList());

    public async Task<bool> UpdateRedirectUrl(string redirectUrl, string clientId, string hashedSecret)
    {
        Models.Client? client = await InMemoryContext.Clients.SingleOrDefaultAsync(o => o.Id == clientId && o.Secret == hashedSecret);
        if (client is null)
            return false;
        client.RedirectUrl = redirectUrl;
        await InMemoryContext.SaveChangesAsync();
        return true;
    }
}
