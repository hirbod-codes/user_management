using System.Security.Authentication;
using Bogus;
using MongoDB.Bson;
using user_management.Dtos.Client;
using user_management.Models;
using user_management.Services;
using user_management.Services.Client;
using user_management.Services.Data;
using Xunit;

namespace user_management.Tests.UnitTests.Controllers;

[Collection("Controller")]
public class ClientControllerTests
{
    public ControllerFixture Fixture { get; private set; }

    public ClientControllerTests(ControllerFixture controllerFixture) => Fixture = controllerFixture;

    private user_management.Controllers.ClientController InstantiateController() => new user_management.Controllers.ClientController(Fixture.IMapper.Object, Fixture.IClientManagement.Object, Fixture.IAuthenticatedByJwt.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void Register_Unauthenticated()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Register(clientCreateDto));
    }

    [Fact]
    public async void Register_Problem()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Models.Client client = new();
        Fixture.IMapper.Setup(o => o.Map<Models.Client>(clientCreateDto)).Returns(client);

        Fixture.IClientManagement.Setup(o => o.Register(client)).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().Register(clientCreateDto), "System failed to register your client.");

        Fixture.IClientManagement.Setup(o => o.Register(client)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().Register(clientCreateDto), "System failed to register your client.");

        Fixture.IClientManagement.Setup(o => o.Register(client)).Throws<RegistrationFailure>();
        HttpAsserts.IsProblem(await InstantiateController().Register(clientCreateDto), "System failed to register your client.");
    }

    [Fact]
    public async void Register_Ok()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Models.Client client = new();
        Fixture.IMapper.Setup(o => o.Map<Models.Client>(clientCreateDto)).Returns(client);
        string? secret = "secret";
        Fixture.IClientManagement.Setup<Task<(Models.Client client, string? notHashedSecret)>>(o => o.Register(client)).Returns(Task.FromResult<(Models.Client client, string? notHashedSecret)>((client, secret)));
        ClientRetrieveDto clientRetrieveDto = new();
        Fixture.IMapper.Setup(o => o.Map<ClientRetrieveDto>(client)).Returns(clientRetrieveDto);

        HttpAsserts<ClientRetrieveDto>.IsOk(await InstantiateController().Register(clientCreateDto), clientRetrieveDto);
    }

    [Fact]
    public async void RetrieveClientPublicInfo_Unauthenticated()
    {
        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().RetrieveClientPublicInfo(id));
    }

    [Fact]
    public async void RetrieveClientPublicInfo_BadRequest()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().RetrieveClientPublicInfo(id));
    }

    [Fact]
    public async void RetrieveClientPublicInfo_NotFound()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Returns(Task.FromResult<Models.Client?>(null));
        HttpAsserts.IsNotFound(await InstantiateController().RetrieveClientPublicInfo(id));
    }

    [Fact]
    public async void RetrieveClientPublicInfo_Ok()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Models.Client? client = new();
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Returns(Task.FromResult<Models.Client?>(client));
        ClientRetrieveDto clientRetrieveDto = new();
        Fixture.IMapper.Setup(o => o.Map<ClientRetrieveDto>(client)).Returns(clientRetrieveDto);
        HttpAsserts<ClientRetrieveDto>.IsOk(await InstantiateController().RetrieveClientPublicInfo(id), clientRetrieveDto);
    }

    [Fact]
    public async void Retrieve_Unauthenticated()
    {
        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Retrieve(id));
    }

    [Fact]
    public async void Retrieve_BadRequest()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().Retrieve(secret));
    }

    [Fact]
    public async void Retrieve_NotFound()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Returns(Task.FromResult<Models.Client?>(null));
        HttpAsserts.IsNotFound(await InstantiateController().Retrieve(secret));
    }

    [Fact]
    public async void Retrieve_Ok()
    {
        Dtos.Client.ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Models.Client? client = new();
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Returns(Task.FromResult<Models.Client?>(client));
        ClientRetrieveDto clientRetrieveDto = new();
        Fixture.IMapper.Setup(o => o.Map<ClientRetrieveDto>(client)).Returns(clientRetrieveDto);
        HttpAsserts<ClientRetrieveDto>.IsOk(await InstantiateController().Retrieve(secret), clientRetrieveDto);
    }

    [Fact]
    public async void Update_Unauthenticated()
    {
        ClientPutDto clientPutDto = new();
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Update(clientPutDto));

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().Update(clientPutDto));
    }

    [Fact]
    public async void Update_Unauthorized()
    {
        ClientPutDto clientPutDto = new();
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new()));
        HttpAsserts.IsUnauthorized(await InstantiateController().Update(clientPutDto));
    }

    [Fact]
    public async void Update_BadRequest()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPutDto clientPutDto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl)).Throws(new ArgumentException("clientId"));
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Update(clientPutDto), "Invalid id for client provided.");

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl)).Throws<DuplicationException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Update(clientPutDto), "The provided redirect url is not unique!");
    }

    [Fact]
    public async void Update_Problem()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPutDto clientPutDto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl)).Throws(new ArgumentException("clientSecret"));
        HttpAsserts.IsProblem(await InstantiateController().Update(clientPutDto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().Update(clientPutDto), "We failed to update this client.");
    }

    [Fact]
    public async void Update_Ok()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPutDto clientPutDto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(clientPutDto.Id, clientPutDto.Secret, clientPutDto.RedirectUrl));
        HttpAsserts.IsOk(await InstantiateController().Update(clientPutDto));
    }

    [Fact]
    public async void Delete_Unauthenticated()
    {
        ClientDeleteDto clientDeleteDto = new() { Id = ObjectId.GenerateNewId().ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Delete(clientDeleteDto));

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().Delete(clientDeleteDto));
    }

    [Fact]
    public async void Delete_Unauthorized()
    {
        ClientDeleteDto clientDeleteDto = new() { Id = ObjectId.GenerateNewId().ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new()));
        HttpAsserts.IsUnauthorized(await InstantiateController().Delete(clientDeleteDto));
    }

    [Fact]
    public async void Delete_BadRequest()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientDeleteDto clientDeleteDto = new() { Id = clientId.ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(clientDeleteDto.Id, clientDeleteDto.Secret)).Throws(new ArgumentException("clientId"));
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Delete(clientDeleteDto), "Invalid id for client provided.");
    }

    [Fact]
    public async void Delete_NotFound()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientDeleteDto clientDeleteDto = new() { Id = clientId.ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(clientDeleteDto.Id, clientDeleteDto.Secret)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().Delete(clientDeleteDto));
    }

    [Fact]
    public async void Delete_Problem()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientDeleteDto clientDeleteDto = new() { Id = clientId.ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(clientDeleteDto.Id, clientDeleteDto.Secret)).Throws(new ArgumentException("secret"));
        HttpAsserts.IsProblem(await InstantiateController().Delete(clientDeleteDto), "Internal server error encountered.");
    }

    [Fact]
    public async void Delete_Ok()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientDeleteDto clientDeleteDto = new() { Id = clientId.ToString(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(clientDeleteDto.Id, clientDeleteDto.Secret));
        HttpAsserts.IsOk(await InstantiateController().Delete(clientDeleteDto));
    }

    [Fact]
    public async void UpdateExposedClient_Unauthenticated()
    {
        ClientExposedDto dto = new();
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateExposedClient(dto));

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateExposedClient(dto));
    }

    [Fact]
    public async void UpdateExposedClient_Unauthorized()
    {
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId, Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new()));
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateExposedClient(dto));

        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId, secret)).Throws<DataNotFoundException>();
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateExposedClient(dto));
    }

    [Fact]
    public async void UpdateExposedClient_Problem()
    {
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId, Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId, secret)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId, secret)).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId, secret)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");
    }

    [Fact]
    public async void UpdateExposedClient_Ok()
    {
        string newSecret = "newSecret";
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId, Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId, secret)).Returns(Task.FromResult(newSecret));

        HttpAsserts<string>.IsOk(await InstantiateController().UpdateExposedClient(dto), newSecret);
    }
}