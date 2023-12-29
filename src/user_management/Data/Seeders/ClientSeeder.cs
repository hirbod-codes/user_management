using Bogus;
using user_management.Models;
using user_management.Utilities;

namespace user_management.Data.Seeders;

public class ClientSeeder
{

    public Client FakeClient(string clientId, out string secret, IEnumerable<Client>? clients = null, DateTime? creationDateTime = null)
    {
        clients ??= Array.Empty<Client>();
        if (creationDateTime == null) creationDateTime = DateTime.UtcNow;

        Faker faker = new("en");

        int safety = 0;
        string redirectUrl;
        string? hashedSecret;
        do
        {
            secret = new StringHelper().GenerateRandomString(128);
            hashedSecret = new StringHelper().HashWithoutSalt(secret);
            redirectUrl = faker.Internet.Url();

            if (
                clients.FirstOrDefault(c => c != null && c.Secret == hashedSecret) == null
                && clients.FirstOrDefault(c => c != null && c.RedirectUrl == redirectUrl) == null
                && clients.FirstOrDefault(c => c != null && c.Id == clientId) == null
            )
                break;

            safety++;
        } while (safety < 200);

        return new Client() { Id = clientId, Secret = hashedSecret!, RedirectUrl = redirectUrl, CreatedAt = (DateTime)creationDateTime, UpdatedAt = (DateTime)creationDateTime };
    }
}
