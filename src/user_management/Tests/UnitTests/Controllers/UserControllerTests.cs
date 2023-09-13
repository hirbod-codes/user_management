using System.Net.Mail;
using Bogus;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Services;
using user_management.Services.Data;
using user_management.Services.Data.User;
using Xunit;
using System.Dynamic;
using System.Security.Authentication;

namespace user_management.Tests.UnitTests.Controllers;

[Collection("Controller")]
public class UserControllerTests
{
    public ControllerFixture Fixture { get; private set; }

    public UserControllerTests(ControllerFixture controllerFixture) => Fixture = controllerFixture;

    private user_management.Controllers.UserController InstantiateController() => new user_management.Controllers.UserController(Fixture.IUserManagement.Object, Fixture.IAuthenticated.Object);

    public static Faker Faker = new("en");

    [Fact]
    public async void FullNameExistenceCheck_Ok()
    {
        string lastName = Faker.Person.FirstName;
        string middleName = Faker.Person.FirstName;
        string firstName = Faker.Person.LastName;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.FullNameExistenceCheck(firstName, middleName, lastName)).Returns(Task.FromResult(true));

        var actionResult = await InstantiateController().FullNameExistenceCheck(firstName, middleName, lastName);

        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void FullNameExistenceCheck_NotFound()
    {
        string lastName = Faker.Person.FirstName;
        string middleName = Faker.Person.FirstName;
        string firstName = Faker.Person.LastName;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.FullNameExistenceCheck(firstName, middleName, lastName)).Returns(Task.FromResult(false));

        var actionResult = await InstantiateController().FullNameExistenceCheck(firstName, middleName, lastName);

        HttpAsserts.IsNotFound(actionResult);
    }

    [Fact]
    public async void FullNameExistenceCheck_BadRequest()
    {
        Fixture.IUserManagement.Setup<Task<bool>>(um => um.FullNameExistenceCheck(null, null, null)).Throws<ArgumentException>();

        var actionResult = await InstantiateController().FullNameExistenceCheck(null, null, null);

        HttpAsserts<string>.IsBadRequest(actionResult, "At least one of the following variables must be provided: firstName, middleName and lastName.");
    }

    [Fact]
    public async void UsernameExistenceCheck_Ok()
    {
        string username = Faker.Person.UserName;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.UsernameExistenceCheck(username)).Returns(Task.FromResult(true));

        var actionResult = await InstantiateController().UsernameExistenceCheck(username);

        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void UsernameExistenceCheck_NotFound()
    {
        string username = Faker.Person.UserName;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.UsernameExistenceCheck(username)).Returns(Task.FromResult(false));

        var actionResult = await InstantiateController().UsernameExistenceCheck(username);

        HttpAsserts.IsNotFound(actionResult);
    }

    [Fact]
    public async void EmailExistenceCheck_Ok()
    {
        string email = Faker.Person.Email;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.EmailExistenceCheck(email)).Returns(Task.FromResult(true));

        var actionResult = await InstantiateController().EmailExistenceCheck(email);

        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void EmailExistenceCheck_NotFound()
    {
        string email = Faker.Person.Email;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.EmailExistenceCheck(email)).Returns(Task.FromResult(false));

        var actionResult = await InstantiateController().EmailExistenceCheck(email);

        HttpAsserts.IsNotFound(actionResult);
    }

    [Fact]
    public async void PhoneNumberExistenceCheck_Ok()
    {
        string phoneNumber = Faker.Person.Phone;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.PhoneNumberExistenceCheck(phoneNumber)).Returns(Task.FromResult(true));

        var actionResult = await InstantiateController().PhoneNumberExistenceCheck(phoneNumber);

        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void PhoneNumberExistenceCheck_NotFound()
    {
        string phoneNumber = Faker.Person.Phone;

        Fixture.IUserManagement.Setup<Task<bool>>(um => um.PhoneNumberExistenceCheck(phoneNumber)).Returns(Task.FromResult(false));

        var actionResult = await InstantiateController().PhoneNumberExistenceCheck(phoneNumber);

        HttpAsserts.IsNotFound(actionResult);
    }

    [Fact]
    public async void Register_Ok()
    {
        UserCreateDto dto = new() { };
        ObjectId id = ObjectId.GenerateNewId();
        User user = new() { Id = id };

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Returns(Task.FromResult(user));

        var actionResult = await InstantiateController().Register(dto);

        HttpAsserts<string>.IsOk(actionResult, id.ToString());
    }

    [Fact]
    public async void Register_Problem()
    {
        IActionResult actionResult;
        UserCreateDto dto = new() { };
        User user = new() { };

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Throws<SmtpException>();
        actionResult = await InstantiateController().Register(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't send the verification message to your email, please try again.");

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Throws<SmtpFailureException>();
        actionResult = await InstantiateController().Register(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't send the verification message to your email, please try again.");

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Throws<RegistrationException>();
        actionResult = await InstantiateController().Register(dto);
        HttpAsserts.IsProblem(actionResult, "System failed to register your account.");

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Throws<DatabaseServerException>();
        actionResult = await InstantiateController().Register(dto);
        HttpAsserts.IsProblem(actionResult, "System failed to register your account.");
    }

    [Fact]
    public async void Register_BadRequest()
    {
        IActionResult actionResult;
        UserCreateDto dto = new() { };
        User user = new() { };

        Fixture.IUserManagement.Setup<Task<User>>(um => um.Register(dto)).Throws<DuplicationException>();
        actionResult = await InstantiateController().Register(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The username or email you chose is no longer unique, please choose another.");
    }

    [Fact]
    public async void SendVerificationEmail_Ok()
    {
        IActionResult actionResult;
        string email = Faker.Person.Email;

        Fixture.IUserManagement.Setup<Task>(um => um.SendVerificationEmail(email));
        actionResult = await InstantiateController().SendVerificationEmail(email);
        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void SendVerificationEmail_Problem()
    {
        IActionResult actionResult;
        string email = Faker.Person.Email;

        Fixture.IUserManagement.Setup<Task>(um => um.SendVerificationEmail(email)).Throws<SmtpException>();
        actionResult = await InstantiateController().SendVerificationEmail(email);
        HttpAsserts.IsProblem(actionResult, "We couldn't send the verification message to your email, please try again.");

        Fixture.IUserManagement.Setup<Task>(um => um.SendVerificationEmail(email)).Throws<SmtpFailureException>();
        actionResult = await InstantiateController().SendVerificationEmail(email);
        HttpAsserts.IsProblem(actionResult, "We couldn't send the verification message to your email, please try again.");

        Fixture.IUserManagement.Setup<Task>(um => um.SendVerificationEmail(email)).Throws<DatabaseServerException>();
        actionResult = await InstantiateController().SendVerificationEmail(email);
        HttpAsserts.IsProblem(actionResult, "Unfortunately we encountered with an internal error.");
    }

    [Fact]
    public async void SendVerificationEmail_NotFound()
    {
        IActionResult actionResult;
        string email = Faker.Person.Email;

        Fixture.IUserManagement.Setup<Task>(um => um.SendVerificationEmail(email)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().SendVerificationEmail(email);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void Activate_Ok()
    {
        IActionResult actionResult;
        Activation activatingUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser));
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts<string>.IsOk(actionResult, "Your account has been registered successfully.");
    }

    [Fact]
    public async void Activate_Problem()
    {
        IActionResult actionResult;
        Activation activatingUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser)).Throws<OperationException>();
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts.IsProblem(actionResult, "We couldn't verify user.");
    }

    [Fact]
    public async void Activate_BadRequest()
    {
        IActionResult actionResult;
        Activation activatingUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser)).Throws<VerificationCodeExpiredException>();
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is expired, please ask for another one.");

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser)).Throws<InvalidVerificationCodeException>();
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts<string>.IsBadRequest(actionResult, "The provided code is not valid.");

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser)).Throws<InvalidPasswordException>();
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts<string>.IsBadRequest(actionResult, "Password is incorrect.");
    }

    [Fact]
    public async void Activate_NotFound()
    {
        IActionResult actionResult;
        Activation activatingUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Activate(activatingUser)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().Activate(activatingUser);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void ChangePassword_Ok()
    {
        IActionResult actionResult;
        ChangePassword dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto));
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts<string>.IsOk(actionResult, "The password changed successfully.");
    }

    [Fact]
    public async void ChangePassword_Problem()
    {
        IActionResult actionResult;
        ChangePassword dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto)).Throws<OperationException>();
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't change the user's password.");
    }

    [Fact]
    public async void ChangePassword_BadRequest()
    {
        IActionResult actionResult;
        ChangePassword dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto)).Throws<PasswordConfirmationMismatchException>();
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "Password confirmation doesn't match with password.");

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto)).Throws<VerificationCodeExpiredException>();
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is expired, please ask for another one.");

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto)).Throws<InvalidVerificationCodeException>();
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is incorrect.");
    }

    [Fact]
    public async void ChangePassword_NotFound()
    {
        IActionResult actionResult;
        ChangePassword dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePassword(dto)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().ChangePassword(dto);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void Login_Ok()
    {
        IActionResult actionResult;
        Login loggingInUser = new() { };
        var expectedResult = new { jwt = "jwt", userId = "id" };
        (string jwt, string userId) tuple;
        tuple.jwt = expectedResult.jwt;
        tuple.userId = expectedResult.userId;

        Fixture.IUserManagement.Setup<Task<(string jwt, string userId)>>(um => um.Login(loggingInUser)).Returns(Task.FromResult<(string jwt, string userId)>(tuple));
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts<object>.IsOk(actionResult, expectedResult);
    }

    [Fact]
    public async void Login_Problem()
    {
        IActionResult actionResult;
        Login loggingInUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Login(loggingInUser)).Throws<OperationException>();
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts.IsProblem(actionResult, "We couldn't log the user in.");
    }

    [Fact]
    public async void Login_BadRequest()
    {
        IActionResult actionResult;
        Login loggingInUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Login(loggingInUser)).Throws<MissingCredentialException>();
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts<string>.IsBadRequest(actionResult, "Incomplete credentials provided.");
    }

    [Fact]
    public async void Login_NotFound()
    {
        IActionResult actionResult;
        Login loggingInUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Login(loggingInUser)).Throws<InvalidPasswordException>();
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with the provided credentials.");

        Fixture.IUserManagement.Setup<Task>(um => um.Login(loggingInUser)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with the provided credentials.");
    }

    [Fact]
    public async void Login_Unauthorized()
    {
        IActionResult actionResult;
        Login loggingInUser = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.Login(loggingInUser)).Throws<UnverifiedUserException>();
        actionResult = await InstantiateController().Login(loggingInUser);
        HttpAsserts<string>.IsUnauthorized(actionResult, "Your account is not activated yet.");
    }

    [Fact]
    public async void Logout_Ok()
    {
        IActionResult actionResult;
        string? id = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(id);

        Fixture.IUserManagement.Setup<Task>(um => um.Logout(id));
        actionResult = await InstantiateController().Logout();
        HttpAsserts.IsOk(actionResult);
    }

    [Fact]
    public async void Logout_Unauthenticated()
    {
        IActionResult actionResult;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);

        actionResult = await InstantiateController().Logout();
        HttpAsserts.IsUnauthenticated(actionResult);
    }

    [Fact]
    public async void Logout_Unauthorized()
    {
        IActionResult actionResult;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("Not JWT");

        actionResult = await InstantiateController().Logout();
        HttpAsserts.IsUnauthorized(actionResult);
    }

    [Fact]
    public async void Logout_Problem()
    {
        IActionResult actionResult;
        string? id = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(id);

        Fixture.IUserManagement.Setup<Task>(um => um.Logout(id)).Throws<OperationException>();
        actionResult = await InstantiateController().Logout();
        HttpAsserts.IsProblem(actionResult, "We couldn't log you out.");
    }

    [Fact]
    public async void Logout_BadRequest()
    {
        IActionResult actionResult;
        string? id = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(id);

        Fixture.IUserManagement.Setup<Task>(um => um.Logout(id)).Throws<ArgumentException>();
        actionResult = await InstantiateController().Logout();
        HttpAsserts.IsBadRequest(actionResult);
    }

    [Fact]
    public async void Logout_NotFound()
    {
        IActionResult actionResult;
        string id = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(id);

        Fixture.IUserManagement.Setup<Task>(um => um.Logout(id)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().Logout();
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find your account.");
    }

    [Fact]
    public async void ChangeUsername_Ok()
    {
        IActionResult actionResult;
        ChangeUsername dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeUsername(dto));
        actionResult = await InstantiateController().ChangeUsername(dto);
        HttpAsserts<string>.IsOk(actionResult, "The username changed successfully.");
    }

    [Fact]
    public async void ChangeUsername_Problem()
    {
        IActionResult actionResult;
        ChangeUsername dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeUsername(dto)).Throws<OperationException>();
        actionResult = await InstantiateController().ChangeUsername(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't change the user's username.");
    }

    [Fact]
    public async void ChangeUsername_BadRequest()
    {
        IActionResult actionResult;
        ChangeUsername dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeUsername(dto)).Throws<VerificationCodeExpiredException>();
        actionResult = await InstantiateController().ChangeUsername(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is expired, please ask for another one.");

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeUsername(dto)).Throws<InvalidVerificationCodeException>();
        actionResult = await InstantiateController().ChangeUsername(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is incorrect.");
    }

    [Fact]
    public async void ChangeUsername_NotFound()
    {
        IActionResult actionResult;
        ChangeUsername dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeUsername(dto)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().ChangeUsername(dto);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void ChangeEmail_Ok()
    {
        IActionResult actionResult;
        ChangeEmail dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeEmail(dto));
        actionResult = await InstantiateController().ChangeEmail(dto);
        HttpAsserts<string>.IsOk(actionResult, "The email changed successfully.");
    }

    [Fact]
    public async void ChangeEmail_Problem()
    {
        IActionResult actionResult;
        ChangeEmail dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeEmail(dto)).Throws<OperationException>();
        actionResult = await InstantiateController().ChangeEmail(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't change the user's email.");
    }

    [Fact]
    public async void ChangeEmail_BadRequest()
    {
        IActionResult actionResult;
        ChangeEmail dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeEmail(dto)).Throws<VerificationCodeExpiredException>();
        actionResult = await InstantiateController().ChangeEmail(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is expired, please ask for another one.");

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeEmail(dto)).Throws<InvalidVerificationCodeException>();
        actionResult = await InstantiateController().ChangeEmail(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is incorrect.");
    }

    [Fact]
    public async void ChangeEmail_NotFound()
    {
        IActionResult actionResult;
        ChangeEmail dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangeEmail(dto)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().ChangeEmail(dto);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void ChangePhoneNumber_Ok()
    {
        IActionResult actionResult;
        ChangePhoneNumber dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePhoneNumber(dto));
        actionResult = await InstantiateController().ChangePhoneNumber(dto);
        HttpAsserts<string>.IsOk(actionResult, "The phone number changed successfully.");
    }

    [Fact]
    public async void ChangePhoneNumber_Problem()
    {
        IActionResult actionResult;
        ChangePhoneNumber dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePhoneNumber(dto)).Throws<OperationException>();
        actionResult = await InstantiateController().ChangePhoneNumber(dto);
        HttpAsserts.IsProblem(actionResult, "We couldn't change the user's phone number.");
    }

    [Fact]
    public async void ChangePhoneNumber_BadRequest()
    {
        IActionResult actionResult;
        ChangePhoneNumber dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePhoneNumber(dto)).Throws<VerificationCodeExpiredException>();
        actionResult = await InstantiateController().ChangePhoneNumber(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is expired, please ask for another one.");

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePhoneNumber(dto)).Throws<InvalidVerificationCodeException>();
        actionResult = await InstantiateController().ChangePhoneNumber(dto);
        HttpAsserts<string>.IsBadRequest(actionResult, "The verification code is incorrect.");
    }

    [Fact]
    public async void ChangePhoneNumber_NotFound()
    {
        IActionResult actionResult;
        ChangePhoneNumber dto = new() { };

        Fixture.IUserManagement.Setup<Task>(um => um.ChangePhoneNumber(dto)).Throws<DataNotFoundException>();
        actionResult = await InstantiateController().ChangePhoneNumber(dto);
        HttpAsserts<string>.IsNotFound(actionResult, "We couldn't find a user with this email.");
    }

    [Fact]
    public async void RemoveClient_Ok()
    {
        string clientId = "clientId";
        string userId = "userId";
        string authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClient(clientId, userId, authorId, forClients));
        HttpAsserts<string>.IsOk(await InstantiateController().RemoveClient(clientId, userId), "The client removed successfully.");
    }

    [Fact]
    public async void RemoveClient_Unauthenticated()
    {
        string clientId = "clientId";
        string userId = "userId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);

        HttpAsserts.IsUnauthenticated(await InstantiateController().RemoveClient(clientId, userId));

        string authorId = "authorId";
        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClient(clientId, userId, authorId, forClients)).Throws<AuthenticationException>();

        HttpAsserts.IsUnauthenticated(await InstantiateController().RemoveClient(clientId, userId));
    }

    [Fact]
    public async void RemoveClient_BadRequest()
    {
        string clientId = "clientId";
        string userId = "userId";
        string authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClient(clientId, userId, authorId, forClients)).Throws(new ArgumentException("clientId"));
        HttpAsserts<string>.IsBadRequest(await InstantiateController().RemoveClient(clientId, userId), "The clientId is not valid.");
    }

    [Fact]
    public async void RemoveClient_NotFound()
    {
        string clientId = "clientId";
        string userId = "userId";
        string authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClient(clientId, userId, authorId, forClients)).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().RemoveClient(clientId, userId), "We couldn't find your account.");
    }

    [Fact]
    public async void RemoveClient_Problem()
    {
        string clientId = "clientId";
        string userId = "userId";
        string authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClient(clientId, userId, authorId, forClients)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().RemoveClient(clientId, userId), "We couldn't remove the client.");
    }

    [Fact]
    public async void RemoveClients_Ok()
    {
        string userId = "userId";
        string authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClients(userId, authorId, forClients));
        HttpAsserts<string>.IsOk(await InstantiateController().RemoveClients(userId), "All of the clients removed successfully.");
    }

    [Fact]
    public async void RemoveClients_Unauthenticated()
    {
        string userId = "userId";
        string? authorId = "authorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().RemoveClients(userId));

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClients(userId, authorId, forClients)).Throws<AuthenticationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().RemoveClients(userId));
    }

    [Fact]
    public async void RemoveClients_BadRequest()
    {
        string userId = "userId";
        string? authorId = "authorId";
        bool forClients = false;
        authorId = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClients(userId, authorId, forClients)).Throws(new ArgumentException("authorId"));
        HttpAsserts<string>.IsBadRequest(await InstantiateController().RemoveClients(userId), "The authorId is not valid.");
    }

    [Fact]
    public async void RemoveClients_NotFound()
    {
        string userId = "userId";
        string? authorId = "id";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClients(userId, authorId, forClients)).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().RemoveClients(userId), "We couldn't find your account.");
    }

    [Fact]
    public async void RemoveClients_Problem()
    {
        string userId = "userId";
        string? authorId = "id";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(authorId);

        Fixture.IUserManagement.Setup<Task>(um => um.RemoveClients(userId, authorId, forClients)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().RemoveClients(userId), "We couldn't remove the clients.");
    }

    [Fact]
    public async void RetrieveById_Ok()
    {
        string userId = "userId";
        string? actorId = ObjectId.GenerateNewId().ToString();
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);


        ObjectId id = ObjectId.GenerateNewId();
        var responseObject = new ExpandoObject() as IDictionary<string, object?>;
        responseObject.Add("_id", id);
        Fixture.IUserManagement.Setup<Task<PartialUser>>(um => um.RetrieveById(actorId, userId, forClients)).Returns(Task.FromResult(new PartialUser() { Id = id }));
        HttpAsserts<object>.IsOk(await InstantiateController().RetrieveById(userId), responseObject);
    }

    [Fact]
    public async void RetrieveById_Unauthenticated()
    {
        string userId = "userId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().RetrieveById(userId));
    }

    [Fact]
    public async void RetrieveById_BadRequest()
    {
        string userId = "userId";
        string? actorId = "actorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task<PartialUser>>(um => um.RetrieveById(actorId, userId, forClients)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().RetrieveById(userId));
    }

    [Fact]
    public async void RetrieveById_NotFound()
    {
        string userId = "userId";
        string? actorId = "actorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task<PartialUser>>(um => um.RetrieveById(actorId, userId, forClients)).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().RetrieveById(userId), "We couldn't find your account.");
    }

    [Fact]
    public async void RetrieveClients_Ok()
    {
        UserClient[] clients = new UserClient[] { };
        PartialUser user = new PartialUser() { Id = ObjectId.GenerateNewId(), Clients = clients };
        string userId = "userId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(userId);

        Fixture.IUserManagement.Setup<Task<PartialUser>>(um => um.RetrieveById(userId, userId, forClients)).Returns(Task.FromResult<PartialUser>(user));
        HttpAsserts<UserClient[]>.IsOk(await InstantiateController().RetrieveClients(), clients);
    }

    [Fact]
    public async void RetrieveClients_Unauthenticated()
    {
        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().RetrieveClients());
    }

    [Fact]
    public async void RetrieveClients_BadRequest()
    {
        string userId = "userId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(userId);

        Fixture.IUserManagement.Setup<Task>(um => um.RetrieveById(userId, userId, forClients)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().RetrieveClients());
    }

    [Fact]
    public async void RetrieveClients_NotFound()
    {
        string userId = "userId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(userId);

        Fixture.IUserManagement.Setup<Task>(um => um.RetrieveById(userId, userId, forClients)).Throws<DataNotFoundException>();
        HttpAsserts<string>.IsNotFound(await InstantiateController().RetrieveClients(), "We couldn't find your account.");
    }

    [Fact]
    public async void RetrieveClients_Unauthorized()
    {
        UserClient[] clients = new UserClient[] { };
        PartialUser user = new PartialUser() { Id = ObjectId.GenerateNewId() };
        string userId = "userId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(userId);

        Fixture.IUserManagement.Setup<Task<PartialUser>>(um => um.RetrieveById(userId, userId, forClients)).Returns(Task.FromResult<PartialUser>(user));
        HttpAsserts.IsUnauthorized(await InstantiateController().RetrieveClients());
    }

    [Fact]
    public async void Retrieve_Ok()
    {
        string logicsString = "string";
        int limit = 5;
        int iteration = 0;
        string? sortBy = null;
        bool ascending = true;
        string actorId = ObjectId.GenerateNewId().ToString();
        bool forClients = false;
        ObjectId id = ObjectId.GenerateNewId();
        List<PartialUser> users = new List<PartialUser>() { new PartialUser() { Id = id } };
        var retrievedUser = new ExpandoObject() as IDictionary<string, object?>;
        retrievedUser.Add("_id", id);

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task<List<PartialUser>>>(um => um.Retrieve(actorId, forClients, logicsString, limit, iteration, sortBy, ascending)).Returns(Task.FromResult<List<PartialUser>>(users));
        HttpAsserts<IEnumerable<object>>.IsOk(await InstantiateController().Retrieve(logicsString, limit, iteration, sortBy, ascending), new List<object>() { retrievedUser });
    }

    [Fact]
    public async void Retrieve_Unauthenticated()
    {
        string logicsString = "string";
        int limit = 5;
        int iteration = 0;
        string? sortBy = null;
        bool ascending = true;
        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);
        HttpAsserts.IsUnauthenticated(await InstantiateController().Retrieve(logicsString, limit, iteration, sortBy, ascending));
    }

    [Fact]
    public async void Retrieve_BadRequest()
    {
        string logicsString = "string";
        int limit = 5;
        int iteration = 0;
        string? sortBy = null;
        bool ascending = true;
        string actorId = "actorId";
        bool forClients = false;

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task<List<PartialUser>>>(um => um.Retrieve(actorId, forClients, logicsString, limit, iteration, sortBy, ascending)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().Retrieve(logicsString, limit, iteration, sortBy, ascending));
    }

    [Fact]
    public async void Retrieve_NotFound()
    {
        string logicsString = "string";
        int limit = 5;
        int iteration = 0;
        string? sortBy = null;
        bool ascending = true;
        string actorId = ObjectId.GenerateNewId().ToString();
        bool forClients = false;
        var responseObject = new ExpandoObject() as IDictionary<string, object?>;
        responseObject.Add("_id", null);

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task<List<PartialUser>>>(um => um.Retrieve(actorId, forClients, logicsString, limit, iteration, sortBy, ascending)).Returns(Task.FromResult<List<PartialUser>>(new List<PartialUser>() { }));
        HttpAsserts.IsNotFound(await InstantiateController().Retrieve(logicsString, limit, iteration, sortBy, ascending));
    }

    [Fact]
    public async void Update_Ok()
    {
        UserPatchDto userPatchDto = new() { FiltersString = "something", UpdatesString = "something" };
        bool forClients = false;
        string actorId = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Update(actorId, userPatchDto, forClients));
        HttpAsserts.IsOk(await InstantiateController().Update(userPatchDto));
    }

    [Fact]
    public async void Update_Unauthenticated()
    {
        UserPatchDto userPatchDto = new() { FiltersString = "something", UpdatesString = "something" };
        bool forClients = false;
        string actorId = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);

        Fixture.IUserManagement.Setup<Task>(um => um.Update(actorId, userPatchDto, forClients)).Throws<OperationException>();
        HttpAsserts.IsUnauthenticated(await InstantiateController().Update(userPatchDto));
    }

    [Fact]
    public async void Update_Problem()
    {
        UserPatchDto userPatchDto = new() { FiltersString = "something", UpdatesString = "something" };
        bool forClients = false;
        string actorId = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Update(actorId, userPatchDto, forClients)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().Update(userPatchDto));
    }

    [Fact]
    public async void Update_BadRequest()
    {
        UserPatchDto userPatchDto;
        bool forClients = false;
        string actorId = "id";

        userPatchDto = new() { FiltersString = "", UpdatesString = "" };
        HttpAsserts.IsBadRequest(await InstantiateController().Update(userPatchDto));

        userPatchDto = new() { FiltersString = "", UpdatesString = "something" };
        HttpAsserts.IsBadRequest(await InstantiateController().Update(userPatchDto));

        userPatchDto = new() { FiltersString = "something", UpdatesString = "" };
        HttpAsserts.IsBadRequest(await InstantiateController().Update(userPatchDto));

        userPatchDto = new() { FiltersString = "empty", UpdatesString = "something" };
        HttpAsserts.IsBadRequest(await InstantiateController().Update(userPatchDto));

        userPatchDto = new() { FiltersString = "something", UpdatesString = "something" };

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Update(actorId, userPatchDto, forClients)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().Update(userPatchDto));
    }

    [Fact]
    public async void Update_NotFound()
    {
        UserPatchDto userPatchDto = new() { FiltersString = "something", UpdatesString = "something" };
        bool forClients = false;
        string actorId = "id";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Update(actorId, userPatchDto, forClients)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().Update(userPatchDto));
    }

    [Fact]
    public void RetrieveMassUpdatableProperties_Ok() { }

    [Fact]
    public void RetrieveMassUpdateProtectedProperties_Ok() { }

    [Fact]
    public async void Delete_Ok()
    {
        string userId = "userId";
        bool forClients = false;
        string actorId = "actorId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Delete(actorId, userId, forClients));
        HttpAsserts.IsOk(await InstantiateController().Delete(userId));
    }

    [Fact]
    public async void Delete_Unauthenticated()
    {
        string userId = "userId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(false);

        HttpAsserts.IsUnauthenticated(await InstantiateController().Delete(userId));
    }

    [Fact]
    public async void Delete_Problem()
    {
        string userId = "userId";
        bool forClients = false;
        string actorId = "actorId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Delete(actorId, userId, forClients)).Throws<OperationException>();
        HttpAsserts.IsProblem(await InstantiateController().Delete(userId));
    }

    [Fact]
    public async void Delete_BadRequest()
    {
        string userId = "userId";
        bool forClients = false;
        string actorId = "actorId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Delete(actorId, userId, forClients)).Throws<ArgumentException>();
        HttpAsserts.IsBadRequest(await InstantiateController().Delete(userId));
    }

    [Fact]
    public async void Delete_NotFound()
    {
        string userId = "userId";
        bool forClients = false;
        string actorId = "actorId";

        Fixture.IAuthenticated.Setup(um => um.IsAuthenticated()).Returns(true);
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticationType()).Returns("JWT");
        Fixture.IAuthenticated.Setup(um => um.GetAuthenticatedIdentifier()).Returns(actorId);

        Fixture.IUserManagement.Setup<Task>(um => um.Delete(actorId, userId, forClients)).Throws<DataNotFoundException>();
        HttpAsserts.IsNotFound(await InstantiateController().Delete(userId));
    }
}