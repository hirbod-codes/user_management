using user_management.Utilities;
using user_management.Controllers.Services;
using user_management.Services.Client;
using MongoDB.Bson;
using user_management.Services.Data;

namespace user_management.Services;

public class ClientManagement : IClientManagement
{
    private readonly IClientRepository _clientRepository;
    private readonly IStringHelper _stringHelper;

    public ClientManagement(IStringHelper stringHelper, IClientRepository clientRepository)
    {
        _stringHelper = stringHelper;
        _clientRepository = clientRepository;
    }

    public async Task<(Models.Client client, string? notHashedSecret)> Register(Models.Client client)
    {
        string? secret = null;
        bool again = false;
        int safety = 0;
        do
        {
            secret = _stringHelper.GenerateRandomString(60);
            secret = _stringHelper.HashWithoutSalt(secret);
            if (secret == null) throw new RegistrationFailure();

            client.Secret = secret;

            try
            {
                client = await _clientRepository.Create(client);
                again = false;
            }
            catch (DuplicationException) { again = true; }

            safety++;
        } while (again && safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return (client, secret);
    }

    public async Task<Models.Client?> RetrieveClientPublicInfo(string id)
    {
        if (!ObjectId.TryParse(id, out ObjectId idObject)) throw new ArgumentException();

        return await _clientRepository.RetrieveById(idObject);
    }

    public async Task<Models.Client?> RetrieveBySecret(string secret)
    {
        string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
        if (hashedSecret == null) throw new ArgumentException();

        return await _clientRepository.RetrieveBySecret(hashedSecret);
    }

    public async Task UpdateRedirectUrl(string clientId, string clientSecret, string redirectUrl)
    {
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException("clientId");

        string? hashedSecret = _stringHelper.HashWithoutSalt(clientSecret);
        if (hashedSecret == null) throw new ArgumentException("clientSecret");

        bool r = await _clientRepository.UpdateRedirectUrl(redirectUrl, clientObjectId, hashedSecret);

        if (r == false) throw new DataNotFoundException();
    }

    public async Task DeleteBySecret(string clientId, string secret)
    {
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) throw new ArgumentException("clientId");

        string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
        if (hashedSecret == null) throw new ArgumentException("secret");

        if (!(await _clientRepository.DeleteBySecret(hashedSecret))) throw new DataNotFoundException();
    }
}