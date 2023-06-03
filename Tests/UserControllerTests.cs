namespace user_management.Tests;

using System.Security.Claims;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using Moq;
using user_management.Authentication.JWT;
using user_management.Controllers;
using user_management.Dtos.User;
using user_management.Models;
using Xunit;
using Microsoft.AspNetCore.Mvc.Infrastructure;

[Collection("Controller")]
public class UserControllerTests
{
    public ControllerFixture ControllerFixture { get; private set; }

    public UserControllerTests(ControllerFixture controllerFixture) => ControllerFixture = controllerFixture;

    private UserController InstantiateUserController() => new UserController(ControllerFixture.IUserRepository.Object, ControllerFixture.IMapper.Object, ControllerFixture.IStringHelper.Object, ControllerFixture.INotificationHelper.Object, ControllerFixture.IAuthHelper.Object, ControllerFixture.IDateTimeProvider.Object);

    [Fact]
    public async Task Api_user_register()
    {
        Faker faker = new("en");

        string password = faker.Internet.Password();
        string hashedPassword = faker.Internet.Password();
        string verificationMessage = faker.Random.String(6);
        string email = faker.Internet.Email();
        DateTime dt = DateTime.UtcNow;
        ObjectId id = ObjectId.GenerateNewId();

        ControllerFixture.IDateTimeProvider.Setup<DateTime>(dt => dt.ProvideUtcNow()).Returns(dt);

        ControllerFixture.IStringHelper.Setup<string>(sh => sh.GenerateRandomString(6)).Returns(verificationMessage);
        ControllerFixture.IStringHelper.Setup<string>(sh => sh.Hash(password)).Returns(hashedPassword);

        UserCreateDto userCreateDto = new();
        userCreateDto.Email = email;
        userCreateDto.Password = password;

        ControllerFixture.INotificationHelper.Setup(nh => nh.SendVerificationMessage(email, verificationMessage));

        User user = new();
        user.Password = hashedPassword;
        user.VerificationSecret = verificationMessage;
        user.VerificationSecretUpdatedAt = dt;
        user.IsVerified = false;

        ControllerFixture.IMapper.Setup<User>(im => im.Map<User>(userCreateDto)).Returns(user);

        User createdUser = new();
        createdUser.Id = id;
        ControllerFixture.IUserRepository.Setup<Task<User>>(iur => iur.Create(user)!).Returns(Task.FromResult<User>(createdUser));

        ActionResult<string> result = await InstantiateUserController().Register(userCreateDto);

        Assert.Equal(id.ToString(), (result.Result as OkObjectResult)!.Value as string);
    }

    [Fact]
    public async Task Api_user_resend_email_verification_message()
    {
        Faker faker = new("en");

        string email = faker.Internet.Email();
        string verificationMessage = faker.Random.String(6);

        ControllerFixture.IStringHelper.Setup<string>(sh => sh.GenerateRandomString(6)).Returns(verificationMessage);

        User user = new();
        user.IsVerified = false;
        user.Email = email;

        ControllerFixture.IUserRepository.Setup<Task<User>>(iur => iur.RetrieveUserByLoginCredentials(email, null)!).Returns(Task.FromResult<User>(user));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateVerificationSecret(verificationMessage, email)!).Returns(Task.FromResult<bool?>(true));

        ControllerFixture.INotificationHelper.Setup(nh => nh.SendVerificationMessage(email, verificationMessage));

        StatusCodeResult? result = (await InstantiateUserController().ResendEmailVerificationMessage(email)) as StatusCodeResult;

        Assert.NotNull(result);
        Assert.Equal<int>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_activate()
    {
        Faker faker = new("en");

        DateTime dt = DateTime.UtcNow;
        string password = faker.Internet.Password();
        string email = faker.Internet.Email();
        string verificationMessage = faker.Random.String(6);
        ObjectId id = ObjectId.GenerateNewId();

        Activation activationUser = new();
        activationUser.Email = email;
        activationUser.VerificationSecret = "same secret";
        activationUser.Password = password;

        User retrievedUser = new();
        retrievedUser.VerificationSecretUpdatedAt = dt;
        retrievedUser.VerificationSecret = "same secret";
        retrievedUser.Password = password;
        retrievedUser.IsVerified = false;
        retrievedUser.Id = id;

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveUserByLoginCredentials(email, null)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.Verify(id)).Returns(Task.FromResult<bool?>(true));

        ControllerFixture.IDateTimeProvider.Setup<DateTime>(idp => idp.ProvideUtcNow()).Returns(dt);

        ControllerFixture.IStringHelper.Setup<bool>(ish => ish.DoesHashMatch(password, password)).Returns(true);

        var result = await InstantiateUserController().Activate(activationUser) as OkObjectResult;

        Assert.NotEmpty((result!.Value as string)!);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_forgot_password()
    {
        Faker faker = new("en");

        string email = faker.Internet.Email();
        string verificationMessage = faker.Random.String(6);

        User retrievedUser = new();
        retrievedUser.Email = email;

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveUserForPasswordChange(email)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.UpdateVerificationSecret(verificationMessage, email)).Returns(Task.FromResult<bool?>(true));

        ControllerFixture.IStringHelper.Setup<string>(ish => ish.GenerateRandomString(6)).Returns(verificationMessage);

        ControllerFixture.INotificationHelper.Setup(inh => inh.SendVerificationMessage(retrievedUser.Email, verificationMessage));

        var result = await InstantiateUserController().ForgotPassword(email) as OkResult;

        Assert.Equal<int>(200, result!.StatusCode);
    }

    [Fact]
    public async Task Api_user_change_password()
    {
        Faker faker = new("en");

        string hashedPassword = faker.Internet.Password();
        string password = faker.Internet.Password();
        string email = faker.Internet.Email();
        DateTime dt = DateTime.UtcNow;

        ChangePassword dto = new();
        dto.Email = email;
        dto.Password = password;
        dto.PasswordConfirmation = password;
        dto.VerificationSecret = "same secret";

        User retrievedUser = new();
        retrievedUser.VerificationSecretUpdatedAt = dt;
        retrievedUser.VerificationSecret = "same secret";

        ControllerFixture.IDateTimeProvider.Setup<DateTime>(idp => idp.ProvideUtcNow()).Returns(dt);

        ControllerFixture.IStringHelper.Setup<string>(ish => ish.Hash(password)).Returns(hashedPassword);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveUserForPasswordChange(dto.Email)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.ChangePassword(dto.Email, hashedPassword)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().ChangePassword(dto) as OkResult;

        Assert.Equal<int>(200, result!.StatusCode);
    }

    [Fact]
    public async Task Api_user_login()
    {
        Faker faker = new("en");

        string hashedPassword = faker.Internet.Password();
        string password = faker.Internet.Password();
        string email = faker.Internet.Email();
        string username = faker.Internet.UserName();
        ObjectId id = ObjectId.GenerateNewId();

        Login dto = new();
        dto.Password = password;
        dto.Email = email;
        dto.Username = username;

        User retrievedUser = new();
        retrievedUser.Id = id;
        retrievedUser.Password = hashedPassword;

        ControllerFixture.IStringHelper.Setup<bool>(ish => ish.DoesHashMatch(retrievedUser.Password!, dto.Password)).Returns(true);

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveUserByLoginCredentials(dto.Email, dto.Username)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.Login(retrievedUser)).Returns(Task.FromResult<bool?>(true));

        Mock<IJWTAuthenticationHandler> ijwt = new();
        ijwt.Setup<string>(ijwt => ijwt.GenerateAuthenticationJWT(retrievedUser.Id.ToString()!)).Returns("jwt");

        var result = await InstantiateUserController().Login(dto, ijwt.Object) as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_logout()
    {
        ObjectId id = ObjectId.GenerateNewId();

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));
        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.Logout(id)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().Logout() as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_remove_client()
    {
        ObjectId id = ObjectId.GenerateNewId();
        ObjectId clientId = ObjectId.GenerateNewId();
        User retrievedUser = new();

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));
        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(id, id, false)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.RemoveClient(retrievedUser, clientId)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().RemoveClient(clientId.ToString()) as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_remove_clients()
    {
        ObjectId id = ObjectId.GenerateNewId();
        User retrievedUser = new();

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));
        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(id, id, false)).Returns(Task.FromResult<User?>(retrievedUser));
        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.RemoveAllClients(retrievedUser)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().RemoveClients() as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_retrieve_by_id()
    {
        ObjectId actorId = ObjectId.GenerateNewId();
        ObjectId id = ObjectId.GenerateNewId();

        User retrievedUser = new();
        retrievedUser.UserPrivileges = new UserPrivileges() { Readers = new Reader[] { new Reader() { Author = Reader.USER, AuthorId = actorId, IsPermitted = true, Fields = new Field[] { new Field() { IsPermitted = true, Name = User.USERNAME } } } } };

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(actorId.ToString()));
        ControllerFixture.IAuthHelper.Setup<string>(iah => iah.GetAuthenticationType(It.IsAny<ClaimsPrincipal>())).Returns("JWT");

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(It.IsAny<ObjectId>(), It.IsAny<ObjectId>(), false)).Returns(Task.FromResult<User?>(retrievedUser));

        var result = await InstantiateUserController().RetrieveById(id.ToString()) as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_retrieve_clients()
    {
        ObjectId actorId = ObjectId.GenerateNewId();

        User retrievedUser = new();
        retrievedUser.Clients = new UserClient[] { };

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(actorId.ToString()));

        ControllerFixture.IUserRepository.Setup<Task<User?>>(iur => iur.RetrieveById(actorId, actorId, false)).Returns(Task.FromResult<User?>(retrievedUser));

        var result = await InstantiateUserController().RetrieveClients() as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_retrieve()
    {
        Faker faker = new("en");
        string logicsString = faker.Random.String(faker.Random.Int(1, 30));
        int limit = faker.Random.Int(1, 10);
        int iteration = faker.Random.Int(1, 10);
        string sortBy = faker.Random.String(faker.Random.Int(1, 10));
        ObjectId id = ObjectId.GenerateNewId();

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));

        ControllerFixture.IUserRepository.Setup<Task<List<User>>>(iur => iur.Retrieve(id, logicsString, limit, iteration, sortBy, faker.Random.Bool(), false)).Returns(Task.FromResult<List<User>>(new List<User> { }));

        var result = await InstantiateUserController().Retrieve(logicsString, limit, iteration, sortBy) as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_update()
    {
        Faker faker = new("en");
        ObjectId id = ObjectId.GenerateNewId();
        UserPatchDto user = new();
        user.UpdatesString = faker.Random.String(faker.Random.Int(1, 30));
        user.FiltersString = faker.Random.String(faker.Random.Int(1, 30));

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));

        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.Update(id, user.FiltersString, user.UpdatesString, false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().Update(user) as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }

    [Fact]
    public async Task Api_user_delete()
    {
        Faker faker = new("en");
        ObjectId id = ObjectId.GenerateNewId();
        ObjectId actorId = ObjectId.GenerateNewId();

        ControllerFixture.IAuthHelper.Setup<Task<string>>(iah => iah.GetIdentifier(It.IsAny<ClaimsPrincipal>(), ControllerFixture.IUserRepository.Object)!).Returns(Task.FromResult<string>(id.ToString()));

        ControllerFixture.IUserRepository.Setup<Task<bool?>>(iur => iur.Delete(It.IsAny<ObjectId>(), It.IsAny<ObjectId>(), false)).Returns(Task.FromResult<bool?>(true));

        var result = await InstantiateUserController().Delete(id.ToString()) as IStatusCodeActionResult;

        Assert.NotNull(result);
        Assert.Equal<int?>(200, result.StatusCode);
    }
}