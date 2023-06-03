namespace user_management.Controllers;
using System.Net.Mail;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
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
    private readonly IStringHelper _stringHelper;
    private readonly INotificationHelper _notificationHelper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserController(IUserRepository userRepository, IMapper mapper, IStringHelper stringHelper, INotificationHelper notificationHelper, IDateTimeProvider dateTimeProvider)
    {
        _mapper = mapper;
        _userRepository = userRepository;
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

}
