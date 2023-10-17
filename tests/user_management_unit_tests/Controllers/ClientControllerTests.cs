using Bogus;
using MongoDB.Bson;
using user_management.Models;
using user_management.Services;
using user_management.Dtos.Client;
using user_management.Services.Data;
using user_management.Services.Data.User;
using user_management.Services.Client;
using System.Security.Authentication;

namespace user_management_unit_tests.Controllers;

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
        ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Register(clientCreateDto));
    }

    [Fact]
    public async void Register_Problem()
    {
        ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Client client = new();
        Fixture.IMapper.Setup(o => o.Map<Client>(clientCreateDto)).Returns(client);

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
        ClientCreateDto clientCreateDto = new();

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Client client = new();
        Fixture.IMapper.Setup(o => o.Map<Client>(clientCreateDto)).Returns(client);
        string? secret = "secret";
        Fixture.IClientManagement.Setup<Task<(Client client, string? notHashedSecret)>>(o => o.Register(client)).Returns(Task.FromResult<(Client client, string? notHashedSecret)>((client, secret)));
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
        ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().RetrieveClientPublicInfo(id));
    }

    [Fact]
    public async void RetrieveClientPublicInfo_NotFound()
    {
        ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Returns(Task.FromResult<Client?>(null));
        HttpAsserts.IsNotFound(await InstantiateController().RetrieveClientPublicInfo(id));
    }

    [Fact]
    public async void RetrieveClientPublicInfo_Ok()
    {
        ClientCreateDto clientCreateDto = new();

        string id = "id";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Client? client = new();
        Fixture.IClientManagement.Setup(o => o.RetrieveClientPublicInfo(id)).Returns(Task.FromResult<Client?>(client));
        ClientPublicInfoRetrieveDto clientPublicInfoRetrieveDto = new();
        Fixture.IMapper.Setup(o => o.Map<ClientPublicInfoRetrieveDto>(client)).Returns(clientPublicInfoRetrieveDto);
        HttpAsserts<ClientPublicInfoRetrieveDto>.IsOk(await InstantiateController().RetrieveClientPublicInfo(id), clientPublicInfoRetrieveDto);
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
        ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().Retrieve(secret));
    }

    [Fact]
    public async void Retrieve_NotFound()
    {
        ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Returns(Task.FromResult<Client?>(null));
        HttpAsserts.IsNotFound(await InstantiateController().Retrieve(secret));
    }

    [Fact]
    public async void Retrieve_Ok()
    {
        ClientCreateDto clientCreateDto = new();

        string secret = "secret";
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Client? client = new();
        Fixture.IClientManagement.Setup(o => o.RetrieveBySecret(secret)).Returns(Task.FromResult<Client?>(client));
        ClientRetrieveDto clientRetrieveDto = new();
        Fixture.IMapper.Setup(o => o.Map<ClientRetrieveDto>(client)).Returns(clientRetrieveDto);
        HttpAsserts<ClientRetrieveDto>.IsOk(await InstantiateController().Retrieve(secret), clientRetrieveDto);
    }

    [Fact]
    public async void Update_Unauthenticated()
    {
        ClientPatchDto dto = new();
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Update(dto));
    }

    [Fact]
    public async void Update_BadRequest()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPatchDto dto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl)).Throws(new ArgumentException("clientId"));
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Update(dto), "Invalid id for client provided.");

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl)).Throws<DuplicationException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Update(dto), "The provided redirect url is not unique!");
    }

    [Fact]
    public async void Update_Problem()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPatchDto dto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl)).Throws(new ArgumentException("clientSecret"));
        HttpAsserts.IsProblem(await InstantiateController().Update(dto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().Update(dto), "We failed to update this client.");
    }

    [Fact]
    public async void Update_Ok()
    {
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientPatchDto dto = new() { Id = clientId.ToString(), RedirectUrl = Faker.Internet.Url(), Secret = Faker.Random.String2(60) };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IAuthenticatedByJwt.Setup(o => o.GetAuthenticated()).Returns(Task.FromResult<User>(new() { Clients = new UserClient[] { new() { ClientId = clientId } } }));
        Fixture.IClientManagement.Setup(o => o.UpdateRedirectUrl(dto.Id, dto.Secret, dto.RedirectUrl));
        HttpAsserts.IsOk(await InstantiateController().Update(dto));
    }

    [Fact]
    public async void Delete_Unauthenticated()
    {
        string secret = Faker.Random.String2(60);
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Delete(secret));
    }

    [Fact]
    public async void Delete_BadRequest()
    {
        string secret = Faker.Random.String2(60);
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(secret)).Throws<ArgumentException>();
        HttpAsserts<string>.IsBadRequest(await InstantiateController().Delete(secret), "Invalid secret provided.");
    }

    [Fact]
    public async void Delete_NotFound()
    {
        string secret = Faker.Random.String2(60);
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(secret)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().Delete(secret));
    }

    [Fact]
    public async void Delete_Ok()
    {
        string secret = Faker.Random.String2(60);
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.DeleteBySecret(secret));
        HttpAsserts.IsOk(await InstantiateController().Delete(secret));
    }

    [Fact]
    public async void UpdateExposedClient_Unauthenticated()
    {
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId.ToString(), Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateExposedClient(dto));

        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(dto.ClientId, dto.Secret)).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().UpdateExposedClient(dto));
    }

    [Fact]
    public async void UpdateExposedClient_Unauthorized()
    {
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId.ToString(), Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId.ToString(), secret)).Throws<DataNotFoundException>();
        HttpAsserts.IsUnauthorized(await InstantiateController().UpdateExposedClient(dto));
    }

    [Fact]
    public async void UpdateExposedClient_Problem()
    {
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId.ToString(), Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId.ToString(), secret)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId.ToString(), secret)).Throws<DuplicationException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");

        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId.ToString(), secret)).Throws<DatabaseServerException>();
        HttpAsserts.IsProblem(await InstantiateController().UpdateExposedClient(dto), "Internal server error encountered.");
    }

    [Fact]
    public async void UpdateExposedClient_Ok()
    {
        string newSecret = "newSecret";
        string secret = "secret";
        ObjectId clientId = ObjectId.GenerateNewId();
        ClientExposedDto dto = new() { ClientId = clientId.ToString(), Secret = secret };
        Fixture.IAuthenticatedByJwt.Setup(o => o.IsAuthenticated()).Returns(value: true);
        Fixture.IClientManagement.Setup(o => o.UpdateExposedClient(clientId.ToString(), secret)).Returns(Task.FromResult(newSecret));

        HttpAsserts<string>.IsOk(await InstantiateController().UpdateExposedClient(dto), newSecret);
    }
}
