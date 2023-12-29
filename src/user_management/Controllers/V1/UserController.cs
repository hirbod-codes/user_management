namespace user_management.Controllers.V1;

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
using user_management.Data;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Cors;
using System.Text.Json;
using user_management.Data.InMemory.Logics;
using user_management.Data.Logics;
using SQLitePCL;

[ApiController]
[ApiVersion("1.0")]
[Route("api")]
public class UserController : ControllerBase
{
    private readonly IUserManagement _userManagement;
    private readonly IAuthenticated _authenticated;
    public UserController(IUserManagement userManagement, IAuthenticated authenticated)
    {
        _userManagement = userManagement;
        _authenticated = authenticated;
    }

    /// <summary>
    /// Check authentication
    /// </summary>
    [Authorize]
    [HttpGet(PATH_GET_IS_AUTHENTICATED)]
    [SwaggerResponse(statusCode: 204, type: typeof(string), description: "This full name already exists, therefor not available.")]
    [SwaggerResponse(statusCode: 401, type: typeof(string), description: "This full name does not exist, therefor available.")]
    public IActionResult IsAuthenticated() => NoContent();

    /// <summary>
    /// Full name existence
    /// </summary>
    /// <remarks>
    /// It's used for registering a user.
    /// </remarks>
    [HttpGet(PATH_GET_FULL_NAME_EXISTENCE_CHECK)]
    [SwaggerResponse(statusCode: 200, type: typeof(string), description: "This full name already exists, therefor not available.")]
    [SwaggerResponse(statusCode: 404, type: typeof(string), description: "This full name does not exist, therefor available.")]
    public async Task<IActionResult> FullNameExistenceCheck([FromQuery] string? firstName, [FromQuery] string? middleName, [FromQuery] string? lastName)
    {
        try { return (await _userManagement.FullNameExistenceCheck(firstName, middleName, lastName)) ? Ok() : NotFound(); }
        catch (ArgumentException) { return BadRequest("At least one of the following variables must be provided: firstName, middleName and lastName."); }
    }

    /// <summary>
    /// Username existence
    /// </summary>
    /// <remarks>
    /// It's used for registering a user.
    /// </remarks>
    [HttpGet(PATH_GET_USERNAME_EXISTENCE_CHECK)]
    [SwaggerResponse(statusCode: 200, type: typeof(string), description: "This username already exists, therefor not available.")]
    [SwaggerResponse(statusCode: 404, type: typeof(string), description: "This username does not exist, therefor available.")]
    public async Task<IActionResult> UsernameExistenceCheck([FromRoute] string username) => (await _userManagement.UsernameExistenceCheck(username)) ? Ok() : NotFound();

    /// <summary>
    /// Email existence
    /// </summary>
    /// <remarks>
    /// It's used for registering a user.
    /// </remarks>
    [HttpGet(PATH_GET_EMAIL_EXISTENCE_CHECK)]
    [SwaggerResponse(statusCode: 200, type: typeof(string), description: "This email already exists, therefor not available.")]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string), description: "This email does not exist, therefor available.")]
    public async Task<IActionResult> EmailExistenceCheck([FromRoute][EmailAddress] string email) => (await _userManagement.EmailExistenceCheck(email)) ? Ok() : NotFound();

    /// <summary>
    /// Phone number existence
    /// </summary>
    /// <remarks>
    /// It's used for registering a user.
    /// </remarks>
    [HttpGet(PATH_GET_PHONE_NUMBER_EXISTENCE_CHECK)]
    [SwaggerResponse(statusCode: 200, type: typeof(string), description: "This phone number already exists, therefor not available.")]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string), description: "This phone number does not exist, therefor available.")]
    public async Task<IActionResult> PhoneNumberExistenceCheck([FromRoute][RegEx(Models.User.PHONE_NUMBER_REGEX)] string phoneNumber) => (await _userManagement.PhoneNumberExistenceCheck(phoneNumber)) ? Ok() : NotFound();

    /// <summary>
    /// Register a user.
    /// </summary>
    [HttpPost(PATH_POST_REGISTER)]
    [SwaggerResponse(statusCode: 200, type: typeof(string), description: "Registered user id in text/plain")]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Register([FromBody] UserCreateDto userDto)
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

    /// <summary>
    /// Send a verification code to the registered user's email.
    /// </summary>
    [HttpPost(PATH_POST_SEND_VERIFICATION_EMAIL)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> SendVerificationEmail([FromQuery][EmailAddress] string email)
    {
        try { await _userManagement.SendVerificationEmail(email); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpFailureException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (DatabaseServerException) { return Problem("Unfortunately we encountered with an internal error."); }

        return Ok();
    }

    /// <summary>
    /// Activate a user account.
    /// </summary>
    /// <remarks>
    /// A verification code must have sent to user's email from our servers within last 6 minutes.  
    /// </remarks>
    [HttpPost(PATH_POST_ACTIVATE)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Activate([FromBody] Activation activatingUser)
    {
        try { await _userManagement.Activate(activatingUser); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The provided code is not valid."); }
        catch (InvalidPasswordException) { return BadRequest("Password is incorrect."); }
        catch (OperationException) { return Problem("We couldn't verify user."); }

        return Ok("Your account has been registered successfully.");
    }

    /// <summary>
    /// Change a registered user's password.
    /// </summary>
    /// <remarks>
    /// A verification code must have sent to user's email from our servers within last 6 minutes.  
    /// </remarks>
    [HttpPost(PATH_POST_CHANGE_PASSWORD)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePassword dto)
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
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 403, type: typeof(string), description: "Unverified accounts won't be able to login.")]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    [EnableCors("third-party-clients")]
    public async Task<IActionResult> Login([FromBody] Login loggingInUser)
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
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string), description: "Invalid credentials.")]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
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

    /// <summary>
    /// Change a user's unverified email.
    /// </summary>
    /// <remarks>
    /// Recently registered users that have not verified yet can change their unverified email.  
    /// </remarks>
    [HttpPost(PATH_POST_CHANGE_UNVERIFIED_EMAIL)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> ChangeUnverifiedEmail([FromBody] ChangeUnverifiedEmail dto)
    {
        try { await _userManagement.ChangeUnverifiedEmail(dto); }
        catch (InvalidPasswordException) { return NotFound(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem("We couldn't change the user's email."); }

        return Ok("The email changed successfully.");
    }

    /// <summary>
    /// Change a registered user's username.
    /// </summary>
    /// <remarks>
    /// A verification code must have sent to user's email from our servers within last 6 minutes.  
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ACCOUNT })]
    [HttpPost(PATH_POST_CHANGE_USERNAME)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> ChangeUsername([FromBody] ChangeUsername dto)
    {
        try { await _userManagement.ChangeUsername(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's username."); }

        return Ok("The username changed successfully.");
    }

    /// <summary>
    /// Change a registered user's email.
    /// </summary>
    /// <remarks>
    /// A verification code must have sent to user's email from our servers within last 6 minutes.  
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ACCOUNT })]
    [HttpPost(PATH_POST_CHANGE_EMAIL)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmail dto)
    {
        try { await _userManagement.ChangeEmail(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's email."); }

        return Ok("The email changed successfully.");
    }

    /// <summary>
    /// Change a registered user's phone number.
    /// </summary>
    /// <remarks>
    /// A verification code must have sent to user's email from our servers within last 3 minutes.  
    /// </remarks>
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ACCOUNT })]
    [HttpPost(PATH_POST_CHANGE_PHONE_NUMBER)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> ChangePhoneNumber([FromBody] ChangePhoneNumber dto)
    {
        try { await _userManagement.ChangePhoneNumber(dto); }
        catch (DataNotFoundException) { return NotFound("We couldn't find a user with this email."); }
        catch (VerificationCodeExpiredException) { return BadRequest("The verification code is expired, please ask for another one."); }
        catch (InvalidVerificationCodeException) { return BadRequest("The verification code is incorrect."); }
        catch (OperationException) { return Problem("We couldn't change the user's phone number."); }

        return Ok("The phone number changed successfully.");
    }

    /// <summary>
    /// Remove an authorized client from list of user's authorized clients.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.DELETE_CLIENT })]
    [HttpPost(PATH_POST_REMOVE_CLIENT)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> RemoveClient([FromQuery] string clientId, [FromQuery] string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.RemoveClient(clientId, userId, _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT"); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the client."); }

        return Ok("The client removed successfully.");
    }

    /// <summary>
    /// Remove all the user's authorized clients.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.DELETE_CLIENTS })]
    [HttpPost(PATH_POST_REMOVE_CLIENTS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> RemoveClients([FromQuery] string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.RemoveClients(userId, _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT"); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (OperationException) { return Problem("We couldn't remove the clients."); }

        return Ok("All of the clients removed successfully.");
    }

    /// <summary>
    /// Retrieve user's data by its ID.
    /// </summary>
    [HttpGet(PATH_GET_USER)]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNT })]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> RetrieveById([ObjectId] string userId)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        PartialUser? user = null;
        try { user = await _userManagement.RetrieveById(_authenticated.GetAuthenticatedIdentifier(), userId, _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (AuthenticationException) { return Unauthorized(); }

        return Ok(user.GetReadable());
    }

    /// <summary>
    /// Retrieve user's authorized clients
    /// </summary>
    [HttpGet(PATH_GET_USER_AUTHORIZED_CLIENTS)]
    [Permissions(Permissions = new string[] { StaticData.READ_CLIENTS })]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> RetrieveClients()
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { return Ok(await _userManagement.RetrieveClientsById(_authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT")); }
        catch (ArgumentException) { return Unauthorized(); }
        catch (AuthenticationException) { return Unauthorized(); }
        catch (DataNotFoundException) { return NotFound("We couldn't find your account."); }
        catch (UnauthorizedAccessException) { return StatusCode(403); }
    }

    /// <summary>
    /// Retrieve list of users.
    /// </summary>
    /// <remarks>
    /// Retrieve list of users with filters and pagination
    /// </remarks> 
    /// <param name="filters">filters</param>
    /// <param name="limit">The number of users per page.</param>
    /// <param name="iteration">Page number starting from 0.</param>
    /// <param name="sortBy">Name of the field to sort users based of.</param>
    /// <param name="ascending">The sort direction.</param>
    [HttpGet(PATH_GET_USERS)]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNT })]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNTS })]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Retrieve([FromRoute] string filtersString, [FromRoute] int limit, [FromRoute] int iteration, [FromRoute] string? sortBy = null, [FromRoute] bool ascending = true)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        Filter? filters;
        try { filters = JsonSerializer.Deserialize<Filter>(filtersString); }
        catch (Exception) { return BadRequest(); }

        MassReadable massReadable = new();
        ValidationResult? validationResult = massReadable.GetValidationResult(filters, new ValidationContext(filters));
        if (validationResult is not null)
            return BadRequest(validationResult);
        // List<ValidationResult> results = new();
        // if (!Validator.TryValidateObject(filters, new ValidationContext(filters), results))
        //     return BadRequest(results);

        List<PartialUser>? users;
        try { users = await _userManagement.Retrieve(_authenticated.GetAuthenticatedIdentifier(), _authenticated.GetAuthenticationType() != "JWT", filters, limit, iteration, sortBy, ascending); }
        catch (ArgumentException) { return BadRequest(); }

        return users.Count == 0 ? NotFound() : Ok(user_management.Models.PartialUser.GetReadable(users));
    }

    [HttpGet("userss")]
    public IActionResult Test([FromQuery] string? a = null, [FromQuery] string? c = null, [FromQuery] string? b = null)
    {
        return Ok($"{{a.Name}} == {{a.IsPermitted}} ||| b: {b} ||| c: {c}");
    }

    /// <summary>
    /// Update multiple users.
    /// </summary>
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNT })]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNTS })]
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ACCOUNT })]
    [Permissions(Permissions = new string[] { StaticData.UPDATE_ACCOUNTS })]
    [HttpPatch(PATH_PATCH_USERS)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Update([FromBody] UserPatchDto userPatchDto)
    {
        if (!userPatchDto.Updates!.Any()) return BadRequest();

        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.Update(_authenticated.GetAuthenticatedIdentifier(), userPatchDto, _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    /// <summary>
    /// Retrieve valid fields when updating multiple users. 
    /// </summary>
    [HttpGet(PATH_GET_USER_MASS_UPDATABLE_PROPERTIES)]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNT })]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Field>))]
    public IActionResult RetrieveMassUpdatableProperties() => Ok(Models.User.GetMassUpdatableFields());

    /// <summary>
    /// Retrieve invalid fields when updating multiple users. 
    /// </summary>
    [HttpGet(PATH_GET_USER_MASS_UPDATE_PROTECTED_PROPERTIES)]
    [Permissions(Permissions = new string[] { StaticData.READ_ACCOUNT })]
    [SwaggerResponse(statusCode: 200, type: typeof(List<Field>))]
    public IActionResult RetrieveMassUpdateProtectedProperties() => Ok(Models.User.GetProtectedFieldsAgainstMassUpdating());

    /// <summary>
    /// Delete a user.
    /// </summary>
    /// <param name="id">The user's id</param>
    [Permissions(Permissions = new string[] { StaticData.DELETE_ACCOUNT })]
    [HttpDelete(PATH_DELETE_USER)]
    [SwaggerResponse(statusCode: 200, type: typeof(string))]
    [SwaggerResponse(statusCode: 400, type: typeof(string))]
    [SwaggerResponse(statusCode: 404, type: typeof(string))]
    public async Task<IActionResult> Delete([FromQuery][ObjectId] string id)
    {
        if (!_authenticated.IsAuthenticated()) return Unauthorized();

        try { await _userManagement.Delete(_authenticated.GetAuthenticatedIdentifier(), id, _authenticated.GetAuthenticationType() != "JWT"); }
        catch (ArgumentException) { return BadRequest(); }
        catch (DataNotFoundException) { return NotFound(); }
        catch (OperationException) { return Problem(); }

        return Ok();
    }

    public const string PATH_GET_IS_AUTHENTICATED = "is-authenticated";
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
    public const string PATH_POST_CHANGE_UNVERIFIED_EMAIL = "change-unverified-email";
    public const string PATH_POST_CHANGE_USERNAME = "change-username";
    public const string PATH_POST_CHANGE_EMAIL = "change-email";
    public const string PATH_POST_CHANGE_PHONE_NUMBER = "change-phone-number";
    public const string PATH_POST_REMOVE_CLIENT = "remove-client";
    public const string PATH_POST_REMOVE_CLIENTS = "remove-clients";
    public const string PATH_GET_USER = "user/{userId}";
    public const string PATH_GET_USER_AUTHORIZED_CLIENTS = "user/authorized-clients";
    public const string PATH_GET_USERS = "users/{filtersString}/{limit}/{iteration}/{sortBy?}/{ascending?}";
    public const string PATH_PATCH_USERS = "users";
    public const string PATH_GET_USER_MASS_UPDATABLE_PROPERTIES = "user/mass-updatable-properties";
    public const string PATH_GET_USER_MASS_UPDATE_PROTECTED_PROPERTIES = "user/mass-update-protected-properties";
    public const string PATH_DELETE_USER = "user";
}
