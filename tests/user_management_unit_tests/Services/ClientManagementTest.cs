using Bogus;
using MongoDB.Bson;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
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
        Client client = new() { Id = ObjectId.GenerateNewId() };
        string secret = "secret";
        string? hashedSecret = "hashedSecret";

        Fixture.IStringHelper.Setup(o => o.GenerateRandomString(60)).Returns(secret);
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);

        await Assert.ThrowsAsync<RegistrationFailure>(async () => await InstantiateService().Register(client));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);
        Fixture.IClientRepository.Setup<Task<Client>>(o => o.Create(client, null)).Throws<DuplicationException>();

        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().Register(client));

        Fixture.IClientRepository.Setup<Task<Client>>(o => o.Create(client, null)).Returns(Task.FromResult<Client>(client));

        (Client client, string? notHashedSecret) result = await InstantiateService().Register(client);

        Assert.Equal(hashedSecret, result.notHashedSecret);
        Assert.Equal(client.Id.ToString(), result.client.Id.ToString());
    }

    [Fact]
    public async void RetrieveClientPublicInfo()
    {
        string id = "ObjectId.GenerateNewId().ToString()";

        await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().RetrieveClientPublicInfo(id));

        id = ObjectId.GenerateNewId().ToString();
        Client client = new() { Id = ObjectId.Parse(id) };
        Fixture.IClientRepository.Setup<Task<Client?>>(o => o.RetrieveById(ObjectId.Parse(id))).Returns(Task.FromResult<Client?>(client));

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
        Client? client = new() { Id = ObjectId.GenerateNewId() };
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

        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl));
        Assert.Equal("clientId", ex.Message);

        id = ObjectId.GenerateNewId().ToString();
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl));
        Assert.Equal("clientSecret", ex.Message);

        string? hashedSecret = "hashedSecret";
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IClientRepository.Setup<Task<bool>>(o => o.UpdateRedirectUrl(redirectUrl, ObjectId.Parse(id), hashedSecret)).Returns(Task.FromResult<bool>(false));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl));

        Fixture.IClientRepository.Setup<Task<bool>>(o => o.UpdateRedirectUrl(redirectUrl, ObjectId.Parse(id), hashedSecret)).Returns(Task.FromResult<bool>(true));
        await InstantiateService().UpdateRedirectUrl(id, secret, redirectUrl);
    }

    [Fact]
    public async void DeleteBySecret()
    {
        string clientId = "clientId";
        string secret = "secret";

        ArgumentException ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().DeleteBySecret(clientId, secret));
        Assert.Equal("clientId", ex.Message);

        clientId = ObjectId.GenerateNewId().ToString();
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        ex = await Assert.ThrowsAsync<ArgumentException>(async () => await InstantiateService().DeleteBySecret(clientId, secret));
        Assert.Equal("secret", ex.Message);

        clientId = ObjectId.GenerateNewId().ToString();
        string? hashedSecret = "hashedSecret";
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IClientRepository.Setup<Task<bool>>(o => o.DeleteBySecret(hashedSecret, null)).Returns(Task.FromResult<bool>(false));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().DeleteBySecret(clientId, secret));

        Fixture.IClientRepository.Setup<Task<bool>>(o => o.DeleteBySecret(hashedSecret, null)).Returns(Task.FromResult<bool>(true));
        await InstantiateService().DeleteBySecret(clientId, secret);
    }

    [Fact]
    public async void UpdateExposedClient()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        string secret = "secret";
        string? hashedSecret = "hashedSecret";
        string? newSecret = "newSecret";
        string? newHashedSecret = "newHashedSecret";

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(secret, "SHA512")).Returns(value: hashedSecret);
        Fixture.IStringHelper.Setup<string?>(o => o.GenerateRandomString(128)).Returns(value: newSecret);
        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(newSecret, "SHA512")).Returns(value: null);
        await Assert.ThrowsAsync<OperationException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));

        Fixture.IStringHelper.Setup<string?>(o => o.HashWithoutSalt(newSecret, "SHA512")).Returns(value: newHashedSecret);
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret, null)).Throws<DuplicationException>();
        await Assert.ThrowsAsync<DuplicationException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret, null)).Throws<DatabaseServerException>();
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret, null)).Returns(Task.FromResult<bool?>(null));
        await Assert.ThrowsAsync<DataNotFoundException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));
        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret, null)).Returns(Task.FromResult<bool?>(false));
        await Assert.ThrowsAsync<DatabaseServerException>(async () => await InstantiateService().UpdateExposedClient(clientId, secret));

        Fixture.IClientRepository.Setup(o => o.ClientExposed(clientId, hashedSecret, newHashedSecret, null)).Returns(Task.FromResult<bool?>(true));
        Assert.Equal(newSecret, await InstantiateService().UpdateExposedClient(clientId, secret));
    }
}
