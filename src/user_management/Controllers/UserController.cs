namespace user_management.Controllers;

using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using user_management.Authorization.Attributes;
using user_management.Authentication;
using user_management.Dtos.User;
using user_management.Models;
using System.ComponentModel.DataAnnotations;
using user_management.Validation.Attributes;
using user_management.Services;
using user_management.Services.Data.User;
using user_management.Services.Data;
using user_management.Controllers.Services;
using System.Security.Authentication;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly IUserManagement _userManagement;
    private readonly IAuthenticated _authenticated;

    public UserController(IUserManagement userManagement, IAuthenticated authenticated)
    {
        _userManagement = userManagement;
        _authenticated = authenticated;
    }

    [HttpGet(PATH_GET_FULL_NAME_EXISTENCE_CHECK)]
    public async Task<IActionResult> FullNameExistenceCheck([FromQuery] string? firstName, [FromQuery] string? middleName, [FromQuery] string? lastName)
    {
        try { return (await _userManagement.FullNameExistenceCheck(firstName, middleName, lastName)) ? Ok() : NotFound(); }
        catch (ArgumentException) { return BadRequest("At least one of the following variables must be provided: firstName, middleName and lastName."); }
    }

    [HttpGet(PATH_GET_USERNAME_EXISTENCE_CHECK)]
    public async Task<IActionResult> UsernameExistenceCheck(string username) => (await _userManagement.UsernameExistenceCheck(username)) ? Ok() : NotFound();

    [HttpGet(PATH_GET_EMAIL_EXISTENCE_CHECK)]
    public async Task<IActionResult> EmailExistenceCheck([EmailAddress] string email) => (await _userManagement.EmailExistenceCheck(email)) ? Ok() : NotFound();

    [HttpGet(PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK)]
    public async Task<IActionResult> PhoneNumberExistenceCheck([RegEx("^[a-z0-9 +-{)(}]{11,}$")] string phoneNumber) => (await _userManagement.PhoneNumberExistenceCheck(phoneNumber)) ? Ok() : NotFound();

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
        try
        {
            return Ok(await _userManagement.Login(loggingInUser));
        }
        catch (MissingCredentialException) { return BadRequest("Incomplete credentials provided."); }
        catch (InvalidPasswordException) { return NotFound("We couldn't find a user with the provided credentials."); }
        catch (UnverifiedUserException) { return StatusCode(403, "Your account is not activated yet."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with the provided credentials."); }
        catch (OperationException) { return Problem("We couldn't log the user in."); }
    }

    [Authorize]
    [HttpPost(PATH_POST_LOGOUT)]
    public async Task<IActionResult> Logout()
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();
        if (_authenticated.GetAuthenticationType() != "JWT") return StatusCode(403);

        try { await _userManagement.Logout(_authenticated.GetAuthenticatedIdentifier()); }
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
    public async Task<IActionResult> RemoveClient([FromQuery] string clientId, [FromQuery] string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.RemoveClient(clientId, userId, _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT"); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException ex) { return BadRequest($"The {ex.Message} is not valid."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the client."); }

        return Ok("The client removed successfully.");
    }

    [Permissions(Permissions = new string[] { "delete_clients" })]
    [HttpPost(PATH_POST_REMOVE_CLIENTS)]
    public async Task<IActionResult> RemoveClients([FromQuery] string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.RemoveClients(userId, _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT"); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException ex) { return BadRequest($"The {ex.Message} is not valid."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the clients."); }

        return Ok("All of the clients removed successfully.");
    }

    [HttpGet(PATH_GET_USER)]
    [Permissions(Permissions = new string[] { "read_account" })]
    public async Task<IActionResult> RetrieveById(string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        PartialUser? user = null;
        try { user = await _userManagement.RetrieveById(_authenticated.GetAuthenticatedIdentifier(), userId, _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }

        return Ok(user.GetReadable());
    }

    [HttpGet(PATH_GET_USER_CLIENTS)]
    [Permissions(Permissions = new string[] { "read_clients" })]
    public async Task<IActionResult> RetrieveClients()
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        PartialUser? user = null;
        try { user = await _userManagement.RetrieveById(_authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }

        if (!user.IsClientsTouched()) return StatusCode(403);

        return Ok(user.Clients);
    }

    [HttpGet(PATH_GET_USERS)]
    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    public async Task<IActionResult> Retrieve(string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        List<PartialUser>? users = null;
        try { users = await _userManagement.Retrieve(_authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT", logicsString, limit, iteration, sortBy, ascending); }
        catch (ArgumentException) { return BadRequest(); }

        return users.Count == 0 ? NotFound() : Ok(user_management.Models.PartialUser.GetReadable(users));
    }

    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    [Permissions(Permissions = new string[] { "update_account" })]
    [Permissions(Permissions = new string[] { "update_accounts" })]
    [HttpPatch(PATH_PATCH_USERS)]
    public async Task<IActionResult> Update(UserPatchDto userPatchDto)
    {
        if (String.IsNullOrWhiteSpace(userPatchDto.UpdatesString) || String.IsNullOrWhiteSpace(userPatchDto.FiltersString) || userPatchDto.FiltersString == "empty") return BadRequest();

        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.Update(_authenticated.GetAuthenticatedIdentifier(), userPatchDto, _authenticated.GetAuthenticationType() != "JWT"); }
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
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.Delete(_authenticated.GetAuthenticatedIdentifier(), id, _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    // putting api paths in constants for later use in tests.
    public const string PATH_GET_FULL_NAME_EXISTENCE_CHECK = "full-name-existence-check";
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
    public const string PATH_POST_REMOVE_CLIENTS = "remove-clients";
    public const string PATH_GET_USER = "user/{id}";
    public const string PATH_GET_USER_CLIENTS = "user/clients";
    public const string PATH_GET_USERS = "users/{logicsString}/{limit}/{iteration}/{sortBy?}/{ascending?}";
    public const string PATH_PATCH_USERS = "users";
    public const string PATH_GET_USER_MASS_UPDATABLE_PROPERTIES = "user/mass-updatable-properties";
    public const string PATH_GET_USER_MASS_UPDATE_PROTECTED_PROPERTIES = "user/mass-update-protected-properties";
    public const string PATH_DELETE_USER = "user";
}
