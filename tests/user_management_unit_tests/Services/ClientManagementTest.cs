using Bogus;
using MongoDB.Bson;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data.Client;
using user_management.Services.Data;

namespace user_management_unit_tests.Services;

[Collection("Service")]
public class ClientManagementTest
{
    public ServiceFixture Fixture { get; private set; }

    public ClientManagementTest(ServiceFixture serviceFixture) => Fixture = serviceFixture;

    private ClientManagement InstantiateService() => new ClientManagement(Fixture.IStringHelper.Object, Fixture.IClientRepository.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void Register()
    {
        Client client = new() { Id = ObjectId.GenerateNewId().ToString() };
        string secret = "secret";
        string? hashedSecret = "hashedSecret";

        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(60)).Returns(secret);
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);

        await Assert.ThrowsAsync<RegistrationFailure>(async () => await InstantiateService().Register(client));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IClientRepository.Setup<Task<Client>>(o => o.Create(client)).Throws<DuplicationException>();

        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().Register(client));

        Fixture.IClientRepository.Setup<Task<Client>>(o => o.Create(client)).Returns(Task.FromResult<Client>(client));

        (Client client, string? notHashedSecret) result = await InstantiateService().Register(client);

        Assert.Equal(secret, result.notHashedSecret);
        Assert.Equal(client.Id.ToString(), result.client.Id.ToString());
    }

    [Fact]
    public async void RetrieveClientPublicInfo()
    {
        string id = ObjectId.GenerateNewId().ToString();
        Client client = new() { Id = id };
        Fixture.IClientRepository.Setup<Task<Client?>>(o => o.RetrieveById(id)).Returns(Task.FromResult<Client?>(client));

        Client? retrievedClient = await InstantiateService().RetrieveClientPublicInfo(id);

        Assert.NotNull(retrievedClient);
        Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
    }

    [Fact]
    public async void RetrieveBySecret()
    {
        string secret = "secret";
        string hashedSecret = "hashedSecret";

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns<string?>(null);
        await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RetrieveBySecret(secret));

        Fixture.IStringHelper.Setup(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Client? client = new() { Id = ObjectId.GenerateNewId().ToString() };
        Fixture.IClientRepository.Setup<Task<Client?>>(o => o.RetrieveBySecret(hashedSecret)).Returns(Task.FromResult<Client?>(client));

        Client? retrievedClient = await InstantiateService().RetrieveBySecret(secret);

        Assert.NotNull(retrievedClient);
        Assert.Equal(client.Id.ToString(), retrievedClient.Id.ToString());
    }

    [Fact]
    public async void UpdateRedirectUrl()
    {
        string id = "null";
        string secret = "null";
        string redirectUrl = "null";

        id = ObjectId.GenerateNewId().ToString();
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl));
        Assert.Equal("clientSecret", ex.ParamName);

        string? hashedSecret = "hashedSecret";
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IClientRepository.Setup<Task<bool>>(o => o.UpdateRedirectUrl(redirectUrl, id, hashedSecret)).Returns(Task.FromResult<bool>(false));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl));

        Fixture.IClientRepository.Setup<Task<bool>>(o => o.UpdateRedirectUrl(redirectUrl, id, hashedSecret)).Returns(Task.FromResult<bool>(true));
        await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl);
    }

    [Fact]
    public async void DeleteBySecret()
    {
        string secret = "secret";

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().DeleteBySecret(secret));
        Assert.Equal("secret", ex.ParamName);

        string? hashedSecret = "hashedSecret";
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IClientRepository.Setup<Task<bool>>(o => o.DeleteBySecret(hashedSecret)).Returns(Task.FromResult<bool>(false));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().DeleteBySecret(secret));

        Fixture.IClientRepository.Setup<Task<bool>>(o => o.DeleteBySecret(hashedSecret)).Returns(Task.FromResult<bool>(true));
        await InstantiateService().DeleteBySecret(secret);
    }

    [Fact]
    public async void UpdateExposedClient()
    {
        string clientId = ObjectId.GenerateNewId().ToString();
        string secret = "secret";
        string? hashedSecret = "hashedSecret";
        string? newSecret = "newSecret";
        string? newHashedSecret = "newHashedSecret";

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IStringHelper.Setup<string?>(o => o.GenerateRandomString(128)).Returns(value: newSecret);
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(newSecret, "SHA512")).Returns(value: null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(newSecret, "SHA512")).Returns(value: newHashedSecret);
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret)).Throws<DuplicationException>();
        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret)).Throws<DatabaseServerException>();
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret)).Returns(Task.FromResult<bool?>(null));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret)).Returns(Task.FromResult<bool?>(false));
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));

        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret)).Returns(Task.FromResult<bool?>(true));
        Assert.Equal(newSecret, await InstantiateService().UpdateExposedClient(clientId.ToString(), secret));
    }
}
