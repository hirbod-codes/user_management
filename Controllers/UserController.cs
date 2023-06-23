namespace user_management.Controllers;

using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Authentication.JWT;
using user_management.Authorization.Attributes;
using user_management.Data.User;
using user_management.Dtos.User;
using user_management.Models;
using user_management.Utilities;

[ApiController]
[Route("api")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private const int EXPIRATION_MINUTES = 6;
    private readonly IMapper _mapper;
    private readonly IUserRepository _userRepository;
    private readonly IAuthHelper _authHelper;
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserController(IUserRepository userRepository, IMapper mapper, IStringHelper stringHelper, INotificationHelper notificationHelper, IAuthHelper authHelper, IDateTimeProvider dateTimeProvider)
    {
        _mapper = mapper;
        _userRepository = userRepository;
        _authHelper = authHelper;
        _notificationHelper = notificationHelper;
        _stringHelper = stringHelper;
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// fullName: <first name>-<middle name>-<last name>, for example: foo-bar-zar or foo--bar for no middle name
    /// </summary>
    [HttpGet("full-name-existence-check/{fullName}")]
    public async Task<IActionResult> FullNameExistenceCheck(string fullName) => (await _userRepository.RetrieveByFullNameForExistenceCheck(fullName)) == null ? NotFound() : Ok();

    [HttpGet("username-existence-check/{username}")]
    public async Task<IActionResult> UsernameExistenceCheck(string username) => (await _userRepository.RetrieveByUsernameForExistenceCheck(username)) == null ? NotFound() : Ok();

    [HttpGet("email-existence-check/{email}")]
    public async Task<IActionResult> EmailExistenceCheck(string email) => (await _userRepository.RetrieveByEmailForExistenceCheck(email)) == null ? NotFound() : Ok();

    [HttpGet("phone-number-existence-check/{phoneNumber}")]
    public async Task<IActionResult> PhoneNumberExistenceCheck(string phoneNumber) => (await _userRepository.RetrieveByPhoneNumberForExistenceCheck(phoneNumber)) == null ? NotFound() : Ok();

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(UserCreateDto user)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        try
        {
            _notificationHelper.SendVerificationMessage(user.Email, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (ObjectDisposedException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (InvalidOperationException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (System.Exception) { return Problem("We couldn't send the verification message to your email, please try again."); }

        User? unverifiedUser = _mapper.Map<User>(user);
        unverifiedUser.Password = _stringHelper.Hash(user.Password);
        unverifiedUser.VerificationSecret = verificationMessage;
        unverifiedUser.VerificationSecretUpdatedAt = _dateTimeProvider.ProvideUtcNow();
        unverifiedUser.IsVerified = false;

        try
        {
            unverifiedUser = await _userRepository.Create(unverifiedUser);
            if (unverifiedUser == null)
                return Problem("System failed to register your account.");
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            return BadRequest("The username or email you chose is no longer unique, please choose another.");
        }

        return Ok(unverifiedUser.Id.ToString());
    }

    [HttpPost("resend-email-verification-message")]
    public async Task<ActionResult> ResendEmailVerificationMessage([FromBody] string email)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        bool? r = await _userRepository.UpdateVerificationSecret(verificationMessage, email);
        if (r == null) return NotFound("We couldn't find a user with this email.");
        if (r == false) return Problem("We couldn't verify the user.");

        try
        {
            _notificationHelper.SendVerificationMessage(email, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (ObjectDisposedException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (InvalidOperationException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (System.Exception) { return Problem("We couldn't send the verification message to your email, please try again."); }

        return Ok();
    }

    [HttpPost("activate")]
    public async Task<ActionResult> Activate(Activation activatingUser)
    {
        User? user = await _userRepository.RetrieveUserByLoginCredentials(activatingUser.Email, null);
        if (user == null) return NotFound("We couldn't find a user with this email.");

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) return BadRequest("The verification code is expired, please ask for another one.");

        if (activatingUser.VerificationSecret != user.VerificationSecret) return BadRequest("The provided email is not valid.");

        if (!_stringHelper.DoesHashMatch(user.Password!, activatingUser.Password)) return BadRequest("Password is incorrect.");

        if ((bool)user.IsVerified!) return Ok("User is already verified.");

        bool? r = await _userRepository.Verify((ObjectId)user.Id!);
        if (r == null) return NotFound("We couldn't find a user with this email.");
        if (r == false) return Problem("We couldn't verify user.");

        return Ok("Your account has been registered successfully.");
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] string email)
    {
        User? user = await _userRepository.RetrieveUserForPasswordChange(email);
        if (user == null) return NotFound("We couldn't find a user with this email.");

        string verificationMessage = _stringHelper.GenerateRandomString(6);

        bool? r = await _userRepository.UpdateVerificationSecretForPasswordChange(verificationMessage, email);
        if (r == null) return NotFound("We couldn't find a user with this email.");
        if (r == false) return Problem("We couldn't add verification secret for user.");

        try
        {
            _notificationHelper.SendVerificationMessage(user.Email!, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (ObjectDisposedException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (InvalidOperationException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (SmtpException) { return Problem("We couldn't send the verification message to your email, please try again."); }
        catch (System.Exception) { return Problem("We couldn't send the verification message to your email, please try again."); }

        return Ok();
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePassword dto)
    {
        if (dto.Password != dto.PasswordConfirmation) return BadRequest("Password confirmation doesn't match with password.");

        User? user = await _userRepository.RetrieveUserForPasswordChange(dto.Email);
        if (user == null) return NotFound("We couldn't find a user with this email.");

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) return BadRequest("The verification code is expired, please ask for another one.");

        if (dto.VerificationSecret != user.VerificationSecret) return BadRequest("The verification code is incorrect.");

        bool? r = await _userRepository.ChangePassword(dto.Email, _stringHelper.Hash(dto.Password));
        if (r == null) return NotFound("We couldn't find a user with this email.");
        if (r == false) return Problem("We couldn't change the user's password.");

        return Ok("the password changed successfully.");
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(Login loggingInUser, [FromServices] IJWTAuthenticationHandler jwtAuthenticationHandler)
    {
        if (loggingInUser.Username == null && loggingInUser.Email == null) return BadRequest("Credentials are not provided.");

        User? user = await _userRepository.RetrieveUserByLoginCredentials(loggingInUser.Email, loggingInUser.Username);
        if (user == null) return NotFound("We couldn't find a user with this email or username.");

        if (!_stringHelper.DoesHashMatch(user.Password!, loggingInUser.Password)) return Unauthorized();

        bool? r = await _userRepository.Login(user);
        if (r == null) return NotFound("We couldn't find a user with this email.");
        if (r == false) return Problem("We couldn't change the user's password.");

        string jwt = jwtAuthenticationHandler.GenerateAuthenticationJWT(user.Id!.ToString()!);

        return Ok(new { jwt = jwt, user = user.GetReadable((ObjectId)user.Id, _mapper) });
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        string? idString = await _authHelper.GetIdentifier(User, _userRepository);
        if (idString == null) return Unauthorized();

        if (!ObjectId.TryParse(idString, out ObjectId id)) return BadRequest();

        if (_authHelper.GetAuthenticationType(User) != "JWT") return BadRequest();

        bool? r = await _userRepository.Logout(id);
        if (r == null) return NotFound("We couldn't find your account.");
        if (r == false) return Problem("We couldn't log you out.");

        return Ok();
    }

    [Permissions(Permissions = new string[] { "delete_client" })]
    [HttpPost("remove-client")]
    public async Task<ActionResult> RemoveClient([FromBody] string clientId)
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return Unauthorized();

        if (!ObjectId.TryParse(id, out ObjectId userId)) return Unauthorized();
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) return BadRequest("The client id is not valid.");

        User? user = await _userRepository.RetrieveById(userId, userId);
        if (user == null) return NotFound("We couldn't find your account.");

        bool? r = await _userRepository.RemoveClient(user, clientObjectId, userId, false);
        if (r == null) return NotFound("You don't have a client with this client id.");
        if (r == false) return Problem("We couldn't remove the client.");

        return Ok("The client removed successfully.");
    }

    [Permissions(Permissions = new string[] { "delete_clients" })]
    [HttpPost("remove-clients")]
    public async Task<ActionResult> RemoveClients()
    {
        if (_authHelper.GetAuthenticationType(User) != "JWT") return StatusCode(403);

        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return Unauthorized();
        if (!ObjectId.TryParse(id, out ObjectId userId)) return Unauthorized();

        User? user = await _userRepository.RetrieveById(userId, userId);
        if (user == null) return NotFound("We couldn't find your account.");

        bool? r = await _userRepository.RemoveAllClients(user, userId, false);
        if (r == null) return NotFound("You don't have any client.");
        if (r == false) return Problem("We couldn't remove your clients.");

        return Ok("All of the clients removed successfully.");
    }

    [HttpGet("user/{id}")]
    [Permissions(Permissions = new string[] { "read_account" })]
    public async Task<ActionResult> RetrieveById(string id)
    {
        string? actorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (actorId == null) return Unauthorized();
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        User? user = await _userRepository.RetrieveById(actorObjectId, ObjectId.Parse(id), _authHelper.GetAuthenticationType(User) != "JWT");
        if (user == null) return NotFound();

        object? content = user.GetReadable(actorObjectId, _mapper, _authHelper.GetAuthenticationType(User) != "JWT");
        if (content == null) return NotFound();

        return Ok(content);
    }

    [NonAction]
    public async Task<User?> RetrieveByIdRpc(string userId, string authorId, bool isClient) => await _userRepository.RetrieveById(ObjectId.Parse(userId), ObjectId.Parse(authorId), isClient);

    [HttpGet("user/clients")]
    [Permissions(Permissions = new string[] { "read_clients" })]
    public async Task<ActionResult> RetrieveClients()
    {
        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return Unauthorized();
        if (!ObjectId.TryParse(id, out ObjectId userId)) return BadRequest();

        User? user = await _userRepository.RetrieveById(userId, userId);
        if (user == null) return NotFound();

        List<UserClient> userClients = user.Clients.ToList();

        return Ok(userClients);
    }

    [HttpGet("users/{logicsString}/{limit}/{iteration}/{sortBy?}/{ascending?}")]
    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    public async Task<ActionResult> Retrieve(string logicsString, int limit, int iteration, string? sortBy, bool ascending = true)
    {
        string? actorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (actorId == null) return Unauthorized();
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        List<User> users = await _userRepository.Retrieve(actorObjectId, logicsString, limit, iteration, sortBy, ascending, _authHelper.GetAuthenticationType(User) != "JWT");

        return Ok(user_management.Models.User.GetReadables(users, actorObjectId, _mapper, _authHelper.GetAuthenticationType(User) != "JWT"));
    }

    [Permissions(Permissions = new string[] { "read_account" })]
    [Permissions(Permissions = new string[] { "read_accounts" })]
    [Permissions(Permissions = new string[] { "update_account" })]
    [Permissions(Permissions = new string[] { "update_accounts" })]
    [HttpPatch("users")]
    public async Task<ActionResult> Update(UserPatchDto userPatchDto)
    {
        if (userPatchDto.UpdatesString == null || userPatchDto.FiltersString == null) return BadRequest();

        string? actorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (actorId == null) return Unauthorized();
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        bool? r = await _userRepository.Update(actorObjectId, userPatchDto.FiltersString, userPatchDto.UpdatesString, _authHelper.GetAuthenticationType(User) != "JWT");
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
    }

    [Permissions(Permissions = new string[] { "delete_account" })]
    [HttpDelete("user")]
    public async Task<ActionResult> Delete([FromBody] string id)
    {
        string? actorId = await _authHelper.GetIdentifier(User, _userRepository);
        if (actorId == null) return Unauthorized();
        if (!ObjectId.TryParse(actorId, out ObjectId actorObjectId)) return BadRequest();

        bool? r = await _userRepository.Delete(actorObjectId, ObjectId.Parse(id), _authHelper.GetAuthenticationType(User) != "JWT");
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
    }
}
