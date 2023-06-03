namespace user_management.Controllers;
using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using user_management.Authentication.JWT;
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
    [HttpGet("full-name-unique-check/{fullName}")]
    public async Task<IActionResult> FullNameUniqueCheck(string fullName) => (await _userRepository.RetrieveByFullNameForUniqueCheck(fullName)) == null ? NotFound() : Ok();

    [HttpGet("username-unique-check/{username}")]
    public async Task<IActionResult> UsernameUniqueCheck(string username) => (await _userRepository.RetrieveByUsernameForUniqueCheck(username)) == null ? NotFound() : Ok();

    [HttpGet("email-unique-check/{email}")]
    public async Task<IActionResult> EmailUniqueCheck(string email) => (await _userRepository.RetrieveByEmailForUniqueCheck(email)) == null ? NotFound() : Ok();

    [HttpGet("phone-number-unique-check/{phoneNumber}")]
    public async Task<IActionResult> PhoneNumberUniqueCheck(string phoneNumber) => (await _userRepository.RetrieveByPhoneNumberForUniqueCheck(phoneNumber)) == null ? NotFound() : Ok();

    [HttpPost("register")]
    public async Task<ActionResult<string>> Register(UserCreateDto user)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        try
        {
            _notificationHelper.SendVerificationMessage(user.Email, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem(); }
        catch (ObjectDisposedException) { return Problem(); }
        catch (InvalidOperationException) { return Problem(); }
        catch (SmtpException) { return Problem(); }
        catch (System.Exception) { return Problem(); }

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
            return Problem("Sorry someone just took the username you chose, please choose another username.");
        }

        return Ok(unverifiedUser.Id.ToString());
    }

    [HttpPost("resend-email-verification-message")]
    public async Task<ActionResult> ResendEmailVerificationMessage([FromBody] string email)
    {
        string verificationMessage = _stringHelper.GenerateRandomString(6);

        User? user = await _userRepository.RetrieveUserByLoginCredentials(email, null);
        if (user == null) return NotFound();

        if (user.IsVerified == true) return BadRequest("The user is already verified.");

        bool? r = await _userRepository.UpdateVerificationSecret(verificationMessage, email);
        if (r == null) return NotFound();
        if (r == false) return Problem();

        try
        {
            _notificationHelper.SendVerificationMessage(user.Email!, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem(); }
        catch (ObjectDisposedException) { return Problem(); }
        catch (InvalidOperationException) { return Problem(); }
        catch (SmtpException) { return Problem(); }
        catch (System.Exception) { return Problem(); }

        return Ok();
    }

    [HttpPost("activate")]
    public async Task<ActionResult> Activate(Activation activatingUser)
    {
        User? user = await _userRepository.RetrieveUserByLoginCredentials(activatingUser.Email, null);
        if (user == null) return NotFound();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) return BadRequest("The verification code is expired, please ask for another one.");

        if (activatingUser.VerificationSecret != user.VerificationSecret) return BadRequest("The provided email is not valid.");

        if (!_stringHelper.DoesHashMatch(user.Password!, activatingUser.Password)) return BadRequest("Password is incorrect.");

        if ((bool)user.IsVerified!) return Ok("User is already verified.");

        bool? r = await _userRepository.Verify((ObjectId)user.Id!);
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok("Your account has been registered successfully.");
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] string email)
    {
        User? user = await _userRepository.RetrieveUserForPasswordChange(email);
        if (user == null) return NotFound();

        string verificationMessage = _stringHelper.GenerateRandomString(6);

        bool? r = await _userRepository.UpdateVerificationSecret(verificationMessage, email);
        if (r == null) return NotFound();
        if (r == false) return Problem();

        try
        {
            _notificationHelper.SendVerificationMessage(user.Email!, verificationMessage);
        }
        catch (ArgumentNullException) { return Problem(); }
        catch (ObjectDisposedException) { return Problem(); }
        catch (InvalidOperationException) { return Problem(); }
        catch (SmtpException) { return Problem(); }
        catch (System.Exception) { return Problem(); }

        return Ok();
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword(ChangePassword dto)
    {
        if (dto.Password != dto.PasswordConfirmation) return BadRequest("Password confirmation doesn't match with password");

        User? user = await _userRepository.RetrieveUserForPasswordChange(dto.Email);
        if (user == null) return NotFound();

        DateTime expirationDateTime = (DateTime)user.VerificationSecretUpdatedAt!;
        expirationDateTime = expirationDateTime.AddMinutes(EXPIRATION_MINUTES);
        if (_dateTimeProvider.ProvideUtcNow() > expirationDateTime) return BadRequest("The verification code is expired, please ask for another one.");

        if (dto.VerificationSecret != user.VerificationSecret) return BadRequest("The verification code is incorrect.");

        bool? r = await _userRepository.ChangePassword(dto.Email, _stringHelper.Hash(dto.Password));
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login(Login loggingInUser, [FromServices] IJWTAuthenticationHandler jwtAuthenticationHandler)
    {
        if (loggingInUser.Username == null && loggingInUser.Email == null) return BadRequest("Credentials are not provided.");

        User? user = await _userRepository.RetrieveUserByLoginCredentials(loggingInUser.Email, loggingInUser.Username);
        if (user == null) return NotFound();

        if (!_stringHelper.DoesHashMatch(user.Password!, loggingInUser.Password)) return Unauthorized();

        bool? r = await _userRepository.Login(user);
        if (r == null) return NotFound();
        if (r == false) return Problem();

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
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
    }

    [Authorize]
    [HttpPost("remove-client")]
    public async Task<ActionResult> RemoveClient(string clientId)
    {
        string authType = _authHelper.GetAuthenticationType(User);
        if (authType != "JWT") return StatusCode(403);

        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return Unauthorized();

        if (!ObjectId.TryParse(id, out ObjectId userId)) return BadRequest();
        if (!ObjectId.TryParse(clientId, out ObjectId clientObjectId)) return BadRequest();

        User? user = await _userRepository.RetrieveById(userId, userId);
        if (user == null) return NotFound();

        bool? r = await _userRepository.RemoveClient(user, clientObjectId);
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
    }

    [Authorize]
    [HttpPost("remove-clients")]
    public async Task<ActionResult> RemoveClients()
    {
        string authType = _authHelper.GetAuthenticationType(User);
        if (authType != "JWT") return StatusCode(403);

        string? id = await _authHelper.GetIdentifier(User, _userRepository);
        if (id == null) return Unauthorized();
        if (!ObjectId.TryParse(id, out ObjectId userId)) return BadRequest();

        User? user = await _userRepository.RetrieveById(userId, userId);
        if (user == null) return NotFound();

        bool? r = await _userRepository.RemoveAllClients(user);
        if (r == null) return NotFound();
        if (r == false) return Problem();

        return Ok();
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

}
