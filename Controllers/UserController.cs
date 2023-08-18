namespace user_management.Controllers;

using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Authorization.Attributes;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;
using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using user_management.Services;
using user_management.Services.Data.User;
using user_management.Services.Data;
using user_management.Controllers.Services;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly IUserManagement _userManagement;
    private readonly IMapper _mapper;
    private readonly IAuthHelper _authHelper;

    public UserController(IUserManagement userManagement, IMapper mapper, IAuthHelper authHelper)
    {
        _userManagement = userManagement;
        _mapper = mapper;
        _authHelper = authHelper;
    }

    [HttpGet(PATH_GET_FULL_NAME_EXISTENCE_CHECK)]
    public async Task<IActionResult> FullNameExistenceCheck(string firstName, string middleName, string lastName) => (await _userManagement.FullNameExistenceCheck(firstName, middleName, lastName)) ? Ok() : NotFound();

    [HttpGet(PATH_GET_USERNAME_EXISTENCE_CHECK)]
    public async Task<IActionResult> UsernameExistenceCheck(string username) => (await _userManagement.UsernameExistenceCheck(username)) ? Ok() : NotFound();

    [HttpGet(PATH_GET_EMAIL_EXISTENCE_CHECK)]
    public async Task<IActionResult> EmailExistenceCheck([EmailAddress] string email) => (await _userManagement.EmailExistenceCheck(email)) ? Ok() : NotFound();

    [HttpGet(PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK)]
    public async Task<IActionResult> PhoneNumberExistenceCheck([RegEx("^[0-9]{11}$")] string phoneNumber) => (await _userManagement.PhoneNumberExistenceCheck(phoneNumber)) ? Ok() : NotFound();

    [HttpPost(PATH_POST_REGISTER)]
    public async Task<IActionResult> Register(UserCreateDto userDto)
    {
        User? unverifiedUser = null;
        try { unverifiedUser = await _userManagement.Register(userDto); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpFailureException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (DuplicationException) { return BadRequest("The username or email you chose is no longer unique, please choose another."); }
        catch (RegistrationException) { return Problem("System failed to register your account."); }
        catch (DatabaseServerException) { return Problem("System failed to register your account."); }

        return Ok(unverifiedUser.Id.ToString());
    }

    [HttpPost(PATH_POST_SEND_VERIFICATION_EMAIL)]
    public async Task<IActionResult> SendVerificationEmail([EmailAddress][FromQuery] string email)
    {
        try { await _userManagement.SendVerificationEmail(email); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpFailureException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (DatabaseServerException) { return Problem("Unfortunately we encountered with an internal error."); }

        return Ok();
    }

    [HttpPost(PATH_POST_ACTIVATE)]
    public async Task<IActionResult> Activate(Activation activatingUser)
    {
        try { await _userManagement.Activate(activatingUser); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The provided code is not valid."); }
        catch (InvalidPasswordException) { return BadRequest("Password is incorrect."); }
        catch (OperationException) { return Problem("We couldn't verify user."); }

        return Ok("Your account has been registered successfully.");
    }

    [HttpPost(PATH_POST_CHANGE_PASSWORD)]
    public async Task<IActionResult> ChangePassword(ChangePassword dto)
    {
        try { await _userManagement.ChangePassword(dto); }
        catch (PasswordConfirmationMismatchException) { return BadRequest("Password confirmation doesn't match with password."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's password."); }

        return Ok("The password changed successfully.");
    }

    [HttpPost(PATH_POST_LOGIN)]
    public async Task<IActionResult> Login(Login loggingInUser)
    {
        try { return Ok(await _userManagement.Login(loggingInUser)); }
        catch (MissingCredentialException) { return BadRequest("No credentials provided."); }
        catch (InvalidPasswordException) { return NotFound("We couldn't find a user with the provided credentials."); }
        catch (UnverifiedUserException) { return StatusCode(403, "Your account is not activated yet."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with the provided credentials."); }
        catch (OperationException) { return Problem("We couldn't change the user's password."); }
    }

    [Authorize]
    [HttpPost(PATH_POST_LOGOUT)]
    public async Task<IActionResult> Logout()
    {
        string? id = await _authHelper.GetIdentifier(User);
        if (id == null) return Unauthorized();

        if (_authHelper.GetAuthenticationType(User) != "JWT") return BadRequest();

        try { await _userManagement.Logout(id); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't log you out."); }

        return Ok();
    }

    [Permissions(Permissions = new string[] { "update_account" })]
    [HttpPost(PATH_POST_CHANGE_USERNAME)]
    public async Task<IActionResult> ChangeUsername(ChangeUsername dto)
    {
        try { await _userManagement.ChangeUsername(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's username."); }

        return Ok("The username changed successfully.");
    }

    [Permissions(Permissions = new string[] { "update_account" })]
    [HttpPost(PATH_POST_CHANGE_EMAIL)]
    public async Task<IActionResult> ChangeEmail(ChangeEmail dto)
    {
        try { await _userManagement.ChangeEmail(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's email."); }

        return Ok("The email changed successfully.");
    }

    [Permissions(Permissions = new string[] { "update_account" })]
    [HttpPost(PATH_POST_CHANGE_PHONE_NUMBER)]
    public async Task<IActionResult> ChangePhoneNumber(ChangePhoneNumber dto)
    {
        try { await _userManagement.ChangePhoneNumber(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's phone number."); }

        return Ok("The phone number changed successfully.");
    }

    [Permissions(Permissions = new string[] { "delete_client" })]
    [HttpPost(PATH_POST_REMOVE_CLIENT)]
    public async Task<IActionResult> RemoveClient([FromQuery] string clientId)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? userId = await _authHelper.GetIdentifier(User);
        if (userId == null) return Unauthorized();

        try { await _userManagement.RemoveClient(clientId, userId); }
        catch (ArgumentException ex) { return ex.Message == "clientId" ? Unauthorized() : BadRequest("The client id is not valid."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the client."); }

        return Ok("The client removed successfully.");
    }

    [Permissions(Permissions = new string[] { "delete_clients" })]
    [HttpPost(PATH_REMOVE_CLIENTS)]
    public async Task<IActionResult> RemoveClients()
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? userId = await _authHelper.GetIdentifier(User);
        if (userId == null) return Unauthorized();

        try { await _userManagement.RemoveClients(userId); }
        catch (ArgumentException ex) { return ex.Message == "clientId" ? Unauthorized() : BadRequest("The client id is not valid."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the clients."); }

        return Ok("All of the clients removed successfully.");
    }

    [HttpGet(PATH_GET_USER)]
    [Permissions(Permissions = new string[] { "read_account" })]
    public async Task<IActionResult> RetrieveById(string userId)
    {
        string? actorId = await _authHelper.GetIdentifier(User);
        if (actorId == null) return Unauthorized();

        User? user = null;

        try { user = await _userManagement.RetrieveById(actorId, userId, _authHelper.GetAuthenticationType(User) != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }

        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        object content = user.GetReadable(actorObjectId, _mapper, _authHelper.GetAuthenticationType(User) != "JWT");

        return Ok(content);
    }

    [HttpGet(PATH_GET_USER_CLIENTS)]
    [Permissions(Permissions = new string[] { "read_clients" })]
    public async Task<IActionResult> RetrieveClients()
    {
        string? id = await _authHelper.GetIdentifier(User);
        if (id == null) return Unauthorized();

        User? user = null;
        try { user = await _userManagement.RetrieveById(id, id, _authHelper.GetAuthenticationType(User) != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }

        List<UserClient> userClients = user.Clients.ToList();

        return Ok(userClients);
    }

    [HttpGet(PATH_GET_USERS)]
    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    public async Task<IActionResult> Retrieve(string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        string? actorId = await _authHelper.GetIdentifier(User);
        if (actorId == null) return Unauthorized();

        List<User>? users = null;
        try { users = await _userManagement.Retrieve(actorId, _authHelper.GetAuthenticationType(User) != "JWT", logicsString, limit, iteration, sortBy, ascending); }
        catch (ArgumentException) { return BadRequest(); }

        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        return users.Count == 0 ? NotFound() : Ok(user_management.Models.User.GetReadables(users, actorObjectId, _mapper, _authHelper.GetAuthenticationType(User) != "JWT"));
    }

    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    [Permissions(Permissions = new string[] { "update_account" })]
    [Permissions(Permissions = new string[] { "update_accounts" })]
    [HttpPatch(PATH_PATCH_USERS)]
    public async Task<IActionResult> Update(UserPatchDto userPatchDto)
    {
        if (String.IsNullOrWhiteSpace(userPatchDto.UpdatesString) || String.IsNullOrWhiteSpace(userPatchDto.FiltersString) || userPatchDto.FiltersString == "empty") return BadRequest();

        string? actorId = await _authHelper.GetIdentifier(User);
        if (actorId == null) return BadRequest("Invalid id");

        try { await _userManagement.Update(actorId, userPatchDto, _authHelper.GetAuthenticationType(User) != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    [HttpGet(PATH_GET_USER_MASS_UPDATABLE_PROPERTIES)]
    [Permissions(Permissions = new string[] { "read_account" })]
    public IActionResult RetrieveMassUpdatableProperties() => Ok(Models.User.GetMassUpdatableFields());

    [HttpGet(PATH_GET_USER_MASS_UPDATE_PROTECTED_PROPERTIES)]
    [Permissions(Permissions = new string[] { "read_account" })]
    public IActionResult RetrieveMassUpdateProtectedProperties() => Ok(Models.User.GetProtectedFieldsAgainstMassUpdating());

    [Permissions(Permissions = new string[] { "delete_account" })]
    [HttpDelete(PATH_DELETE_USER)]
    public async Task<IActionResult> Delete([ObjectIdAttribute][FromQuery] string id)
    {
        string? actorId = await _authHelper.GetIdentifier(User);
        if (actorId == null) return Unauthorized();

        try { await _userManagement.Delete(actorId, id, _authHelper.GetAuthenticationType(User) != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    // putting api paths in constants for later use in tests.
    public const string PATH_GET_FULL_NAME_EXISTENCE_CHECK = "full-name-existence-check/{firstName}/{middleName}/{lastName}";
    public const string PATH_GET_USERNAME_EXISTENCE_CHECK = "username-existence-check/{username}";
    public const string PATH_GET_EMAIL_EXISTENCE_CHECK = "email-existence-check/{email}";
    public const string PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK = "phone-number-existence-check/{phoneNumber}";
    public const string PATH_POST_REGISTER = "register";
    public const string PATH_POST_SEND_VERIFICATION_EMAIL = "send-verification-email";
    public const string PATH_POST_ACTIVATE = "activate";
    public const string PATH_POST_CHANGE_PASSWORD = "change-password";
    public const string PATH_POST_LOGIN = "login";
    public const string PATH_POST_LOGOUT = "logout";
    public const string PATH_POST_CHANGE_USERNAME = "change-username";
    public const string PATH_POST_CHANGE_EMAIL = "change-email";
    public const string PATH_POST_CHANGE_PHONE_NUMBER = "change-phone-number";
    public const string PATH_POST_REMOVE_CLIENT = "remove-client";
    public const string PATH_REMOVE_CLIENTS = "remove-clients";
    public const string PATH_GET_USER = "user/{id}";
    public const string PATH_GET_USER_CLIENTS = "user/clients";
    public const string PATH_GET_USERS = "users/{logicsString}/{limit}/{iteration}/{sortBy?}/{ascending?}";
    public const string PATH_PATCH_USERS = "users";
    public const string PATH_GET_USER_MASS_UPDATABLE_PROPERTIES = "user/mass-updatable-properties";
    public const string PATH_GET_USER_MASS_UPDATE_PROTECTED_PROPERTIES = "user/mass-update-protected-properties";
    public const string PATH_DELETE_USER = "user";
}
