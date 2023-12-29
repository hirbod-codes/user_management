using user_management.Utilities;
using user_management.Controllers.Services;
using user_management.Services.Data.Client;
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
        string? secret;
        bool again = false;
        int safety = 0;
        do
        {
            secret = _stringHelper.GenerateRandomString(60);
            string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
            if (hashedSecret == null) throw new RegistrationFailure();

            client.Secret = hashedSecret;

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

    public async Task<Models.Client?> RetrieveClientPublicInfo(string id) => await _clientRepository.RetrieveById(id);

    public async Task<Models.Client?> RetrieveBySecret(string secret)
    {
        string? hashedSecret = _stringHelper.HashWithoutSalt(secret);
        if (hashedSecret == null) throw new ArgumentException();

        return await _clientRepository.RetrieveBySecret(hashedSecret);
    }

    public async Task UpdateRedirectUrl(string clientId, string clientSecret, string redirectUrl)
    {
        string hashedSecret = _stringHelper.HashWithoutSalt(clientSecret) ?? throw new ArgumentException(null, nameof(clientSecret));

        bool r = await _clientRepository.UpdateRedirectUrl(redirectUrl, clientId, hashedSecret);

        if (r == false) throw new DataNotFoundException();
    }

    public async Task DeleteBySecret(string secret)
    {
        string hashedSecret = _stringHelper.HashWithoutSalt(secret) ?? throw new ArgumentException(null, paramName: nameof(secret));
        if (!await _clientRepository.DeleteBySecret(hashedSecret)) throw new DataNotFoundException();
    }

    public async Task<string> UpdateExposedClient(string clientId, string secret)
    {
        string hashedSecret = _stringHelper.HashWithoutSalt(secret) ?? throw new OperationException();

        int safety = 0;
        string newSecret;
        do
        {
            newSecret = _stringHelper.GenerateRandomString(128);
            string newHashedSecret = _stringHelper.HashWithoutSalt(newSecret) ?? throw new OperationException();

            bool? r = false;
            try { r = await _clientRepository.ClientExposed(clientId, hashedSecret, newHashedSecret); }
            catch (DuplicationException) { safety++; continue; }

            if (r == null) throw new DataNotFoundException();
            if (r == false) throw new DatabaseServerException();
            if (r == true) break;
            safety++;
        } while (safety < 200);

        if (safety >= 200) throw new DuplicationException();

        return newSecret;
    }
}
