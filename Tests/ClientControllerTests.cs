namespace user_management.Tests;

using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using MongoDB.Bson;
using Moq;
using user_management.Controllers;
using user_management.Dtos.Client;
using user_management.Models;
using Xunit;

[Collection("Controller")]
public class ClientControllerTests
{
    public ControllerFixture ControllerFixture { get; private set; }

    public ClientControllerTests(ControllerFixture controllerFixture) => ControllerFixture = controllerFixture;

    private ClientController InstantiateController() => new ClientController(ControllerFixture.IMapper.Object, ControllerFixture.IClientRepository.Object, ControllerFixture.IStringHelper.Object, ControllerFixture.IAuthHelper.Object);

    [Fact]
    public async Task Api_client_register()
    {
        Faker faker = new("en");
        string secret = faker.Random.String(60);
        string hashedSecret = faker.Random.String(60);

        ClientCreateDto clientCreateDto = new();

        ClientRetrieveDto clientRetrieveDto = new();

        Client client = new();

        Client createdClient = new();

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IMapper.Setup<Client>(c => c.Map<Client>(clientCreateDto)).Returns(client);
        ControllerFixture.IMapper.Setup<ClientRetrieveDto>(c => c.Map<ClientRetrieveDto>(It.IsAny<Client>())).Returns(clientRetrieveDto);

        ControllerFixture.IStringHelper.Setup<string>(ish => ish.GenerateRandomString(60)).Returns(secret);
        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);

        ControllerFixture.IClientRepository.Setup<Task<Client>>(icr => icr.Create(client)).Returns(Task.FromResult<Client>(createdClient));

        var result = await InstantiateController().Register(clientCreateDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
        Assert.Equal(secret, ((result as OkObjectResult)!.Value as ClientRetrieveDto)!.Secret);
    }

    [Fact]
    public async Task Api_client_retrieve()
    {
        Faker faker = new("en");
        string secret = faker.Random.String(60);
        string hashedSecret = faker.Random.String(60);

        Client createdClient = new();

        ClientRetrieveDto clientRetrieveDto = new();

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);

        ControllerFixture.IClientRepository.Setup<Task<Client?>>(icr => icr.RetrieveBySecret(hashedSecret)).Returns(Task.FromResult<Client?>(createdClient));

        ControllerFixture.IMapper.Setup<ClientRetrieveDto>(c => c.Map<ClientRetrieveDto>(It.IsAny<Client>())).Returns(clientRetrieveDto);

        var result = await InstantiateController().Retrieve(secret);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
        Assert.NotNull(((result as OkObjectResult)!.Value as ClientRetrieveDto));
    }

    [Fact]
    public async Task Api_client_update()
    {
        Faker faker = new("en");
        ObjectId id = ObjectId.GenerateNewId();
        string redirectUrl = faker.Internet.Url();
        string secret = faker.Random.String(60);
        string hashedSecret = faker.Random.String(60);

        ClientPutDto clientPutDto = new() { Id = id.ToString(), Secret = secret, RedirectUrl = redirectUrl };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);

        ControllerFixture.IClientRepository.Setup<Task<bool>>(icr => icr.UpdateRedirectUrl(redirectUrl, id, hashedSecret)).Returns(Task.FromResult<bool>(true));

        var result = await InstantiateController().Update(clientPutDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }

    [Fact]
    public async Task Api_client_delete()
    {
        Faker faker = new("en");
        ObjectId id = ObjectId.GenerateNewId();
        string secret = faker.Random.String(60);
        string hashedSecret = faker.Random.String(60);

        Client client = new();
        client.Secret = hashedSecret;

        ClientDeleteDto clientDeleteDto = new() { Id = id.ToString(), Secret = secret };

        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IStringHelper.Setup<string?>(ish => ish.HashWithoutSalt(secret, "SHA512")).Returns(hashedSecret);

        ControllerFixture.IClientRepository.Setup<Task<Client?>>(icr => icr.RetrieveById(id)).Returns(Task.FromResult<Client?>(client));
        ControllerFixture.IClientRepository.Setup<Task<bool>>(icr => icr.DeleteBySecret(hashedSecret)).Returns(Task.FromResult(true));

        var result = await InstantiateController().Delete(clientDeleteDto);

        Assert.Equal<int?>(200, (result as IStatusCodeActionResult)!.StatusCode);
    }
}